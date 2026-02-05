using QuizPlatform.Application.DTOs.Exam;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;
using QuizPlatform.Domain.ValueObjects;

namespace QuizPlatform.Application.Services;

/// <summary>
/// Service for exam operations including auto-grading.
/// </summary>
public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IExamAttemptRepository _attemptRepository;
    private readonly ISubmissionRepository _submissionRepository;

    public ExamService(
        IExamRepository examRepository,
        IQuestionRepository questionRepository,
        IExamAttemptRepository attemptRepository,
        ISubmissionRepository submissionRepository)
    {
        _examRepository = examRepository;
        _questionRepository = questionRepository;
        _attemptRepository = attemptRepository;
        _submissionRepository = submissionRepository;
    }

    public async Task<IEnumerable<ExamListDto>> GetActiveExamsAsync()
    {
        var exams = await _examRepository.GetActiveExamsAsync();
        return exams.Select(e => new ExamListDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Duration = e.Duration,
            TotalScore = e.TotalScore,
            QuestionCount = e.QuestionCount
        });
    }

    public async Task<StartExamResponse> StartExamAsync(string userId, string examId)
    {
        // Get exam details first
        var examEntity = await _examRepository.GetByIdAsync(examId);
        if (examEntity == null || !examEntity.IsActive)
        {
            throw new InvalidOperationException("Exam not found or not active");
        }

        // Check if there's an active attempt
        var existingAttempt = await _attemptRepository.GetActiveAttemptAsync(userId, examId);
        if (existingAttempt != null)
        {
            // Return existing attempt if still valid
            if (existingAttempt.ExpiredAt > DateTime.UtcNow)
            {
                // Get the questions that were assigned to this attempt
                IEnumerable<Question> attemptQuestions;
                if (examEntity.IsRandom)
                {
                    // For random exams, we would need to store assigned question IDs
                    // For now, get questions by exam's QuestionIds or random
                    attemptQuestions = examEntity.QuestionIds.Any() 
                        ? await _questionRepository.GetByIdsAsync(examEntity.QuestionIds)
                        : await _questionRepository.GetRandomQuestionsByCategoriesAsync(examEntity.QuestionCount, examEntity.Categories);
                }
                else
                {
                    attemptQuestions = await _questionRepository.GetByIdsAsync(examEntity.QuestionIds);
                }
                
                return new StartExamResponse
                {
                    AttemptId = existingAttempt.Id,
                    ExamId = examId,
                    Title = examEntity.Title,
                    Duration = examEntity.Duration,
                    StartedAt = existingAttempt.StartedAt,
                    ExpiredAt = existingAttempt.ExpiredAt,
                    Questions = attemptQuestions.Select(MapToQuestionDto).ToList()
                };
            }
            
            // Mark expired attempt as timeout
            existingAttempt.Status = AttemptStatus.Timeout;
            await _attemptRepository.UpdateAsync(existingAttempt.Id, existingAttempt);
        }

        // Get questions based on exam settings
        IEnumerable<Question> examQuestions;
        if (examEntity.IsRandom)
        {
            // Random selection: use Categories filter if specified
            examQuestions = await _questionRepository.GetRandomQuestionsByCategoriesAsync(
                examEntity.QuestionCount, 
                examEntity.Categories);
        }
        else
        {
            // Manual selection: use the stored QuestionIds
            if (examEntity.QuestionIds.Any())
            {
                examQuestions = await _questionRepository.GetByIdsAsync(examEntity.QuestionIds);
            }
            else
            {
                // Fallback to legacy behavior (questions linked via ExamId field)
                examQuestions = await _questionRepository.GetByExamIdAsync(examId);
            }
        }

        // Create new attempt
        var attempt = new ExamAttempt
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ExamId = examId,
            StartedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(examEntity.Duration),
            Status = AttemptStatus.Doing,
            CurrentAnswers = new List<AttemptAnswer>()
        };

        await _attemptRepository.CreateAsync(attempt);

        return new StartExamResponse
        {
            AttemptId = attempt.Id,
            ExamId = examId,
            Title = examEntity.Title,
            Duration = examEntity.Duration,
            StartedAt = attempt.StartedAt,
            ExpiredAt = attempt.ExpiredAt,
            Questions = examQuestions.Select(MapToQuestionDto).ToList()
        };
    }

    public async Task SaveProgressAsync(string attemptId, SaveProgressRequest request)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);
        if (attempt == null || attempt.Status != AttemptStatus.Doing)
        {
            throw new InvalidOperationException("Attempt not found or already submitted");
        }

        attempt.CurrentAnswers = request.Answers;
        await _attemptRepository.UpdateAsync(attemptId, attempt);
    }

    public async Task<ExamResultDto> SubmitExamAsync(string userId, string attemptId, SubmitExamRequest request)
    {
        // Validate attempt
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);
        if (attempt == null)
        {
            throw new InvalidOperationException("Attempt not found");
        }

        if (attempt.UserId != userId)
        {
            throw new InvalidOperationException("Unauthorized");
        }

        if (attempt.Status != AttemptStatus.Doing)
        {
            throw new InvalidOperationException("Exam already submitted");
        }

        // Get exam and questions
        var exam = await _examRepository.GetByIdAsync(attempt.ExamId);
        
        // Get questions by the IDs submitted in the answers
        var questionIds = request.Answers.Select(a => a.QuestionId).Distinct();
        var questions = (await _questionRepository.GetByIdsAsync(questionIds)).ToList();

        // Auto-grading logic
        var gradedAnswers = new List<GradedAnswer>();
        decimal totalScore = 0;

        foreach (var answer in request.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            bool isCorrect = CompareAnswers(answer.SelectedAnswers, question.CorrectAnswers);
            decimal score = isCorrect ? question.Score : 0;
            totalScore += score;

            gradedAnswers.Add(new GradedAnswer
            {
                QuestionId = question.Id,
                SelectedAnswers = answer.SelectedAnswers,
                IsCorrect = isCorrect,
                Score = score
            });
        }

        // Create submission
        var submission = new Submission
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ExamId = attempt.ExamId,
            Answers = gradedAnswers,
            TotalScore = totalScore,
            SubmittedAt = DateTime.UtcNow,
            TimeTaken = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds
        };

        await _submissionRepository.CreateAsync(submission);

        // Update attempt status
        attempt.Status = AttemptStatus.Submitted;
        await _attemptRepository.UpdateAsync(attemptId, attempt);

        // Build result
        return BuildExamResult(submission, exam!, questions);
    }

    public async Task<ExamResultDto?> GetResultAsync(string submissionId)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null) return null;

        var exam = await _examRepository.GetByIdAsync(submission.ExamId);
        
        // Get questions by the IDs stored in the submission answers
        var questionIds = submission.Answers.Select(a => a.QuestionId).Distinct();
        var questions = (await _questionRepository.GetByIdsAsync(questionIds)).ToList();

        return BuildExamResult(submission, exam!, questions);
    }

    public async Task<IEnumerable<ExamResultDto>> GetUserHistoryAsync(string userId)
    {
        var submissions = await _submissionRepository.GetByUserIdAsync(userId);
        var results = new List<ExamResultDto>();

        foreach (var submission in submissions)
        {
            var exam = await _examRepository.GetByIdAsync(submission.ExamId);
            if (exam != null)
            {
                results.Add(new ExamResultDto
                {
                    SubmissionId = submission.Id,
                    ExamTitle = exam.Title,
                    TotalScore = submission.TotalScore,
                    MaxScore = exam.TotalScore,
                    TimeTaken = submission.TimeTaken,
                    SubmittedAt = submission.SubmittedAt
                });
            }
        }

        return results;
    }

    private static bool CompareAnswers(List<string> userAnswers, List<string> correctAnswers)
    {
        if (userAnswers.Count != correctAnswers.Count) return false;
        return userAnswers.OrderBy(x => x).SequenceEqual(correctAnswers.OrderBy(x => x));
    }

    private static QuestionDto MapToQuestionDto(Question q)
    {
        return new QuestionDto
        {
            Id = q.Id,
            Content = q.Content,
            Type = q.Type.ToString(),
            Options = q.Options,
            Score = q.Score,
            Order = q.Order,
            AudioUrl = q.AudioUrl,
            ImageUrl = q.ImageUrl
        };
    }

    private static ExamResultDto BuildExamResult(Submission submission, Exam exam, List<Question> questions)
    {
        var questionResults = new List<QuestionResultDto>();

        foreach (var question in questions)
        {
            var answer = submission.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            questionResults.Add(new QuestionResultDto
            {
                Id = question.Id,
                Content = question.Content,
                Options = question.Options,
                CorrectAnswers = question.CorrectAnswers,
                UserAnswers = answer?.SelectedAnswers ?? new List<string>(),
                IsCorrect = answer?.IsCorrect ?? false,
                Score = answer?.Score ?? 0,
                Explanation = question.Explanation
            });
        }

        return new ExamResultDto
        {
            SubmissionId = submission.Id,
            ExamTitle = exam.Title,
            TotalScore = submission.TotalScore,
            MaxScore = exam.TotalScore,
            TimeTaken = submission.TimeTaken,
            SubmittedAt = submission.SubmittedAt,
            Questions = questionResults
        };
    }
}
