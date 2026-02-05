using QuizPlatform.Application.DTOs.Exam;
using QuizPlatform.Application.Interfaces;

namespace QuizPlatform.Application.Services;

/// <summary>
/// Service for managing exam history and submissions.
/// Implements business logic for retrieving exam history with proper data transformation.
/// </summary>
public class ExamHistoryService : IExamHistoryService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IExamRepository _examRepository;
    private readonly IUserRepository _userRepository;
    private readonly IQuestionRepository _questionRepository;

    public ExamHistoryService(
        ISubmissionRepository submissionRepository,
        IExamRepository examRepository,
        IUserRepository userRepository,
        IQuestionRepository questionRepository)
    {
        _submissionRepository = submissionRepository;
        _examRepository = examRepository;
        _userRepository = userRepository;
        _questionRepository = questionRepository;
    }

    public async Task<ExamHistoryListDto> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        var submissions = (await _submissionRepository.GetByUserIdAsync(userId)).ToList();
        
        // Sort by submission date (most recent first)
        var sortedSubmissions = submissions.OrderByDescending(s => s.SubmittedAt).ToList();
        
        var totalCount = sortedSubmissions.Count;
        var pagedSubmissions = sortedSubmissions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var historyItems = new List<ExamHistoryDto>();
        
        foreach (var submission in pagedSubmissions)
        {
            var historyItem = await MapToHistoryDtoAsync(submission);
            if (historyItem != null)
            {
                historyItems.Add(historyItem);
            }
        }

        return new ExamHistoryListDto
        {
            Items = historyItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ExamHistoryListDto> GetAllHistoryAsync(int page = 1, int pageSize = 20, string? examId = null, string? userId = null)
    {
        var submissions = (await _submissionRepository.GetAllAsync()).ToList();

        // Apply filters
        if (!string.IsNullOrEmpty(examId))
        {
            submissions = submissions.Where(s => s.ExamId == examId).ToList();
        }

        if (!string.IsNullOrEmpty(userId))
        {
            submissions = submissions.Where(s => s.UserId == userId).ToList();
        }

        // Sort by submission date (most recent first)
        var sortedSubmissions = submissions.OrderByDescending(s => s.SubmittedAt).ToList();
        
        var totalCount = sortedSubmissions.Count;
        var pagedSubmissions = sortedSubmissions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var historyItems = new List<ExamHistoryDto>();
        
        foreach (var submission in pagedSubmissions)
        {
            var historyItem = await MapToHistoryDtoAsync(submission);
            if (historyItem != null)
            {
                historyItems.Add(historyItem);
            }
        }

        return new ExamHistoryListDto
        {
            Items = historyItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SubmissionDto?> GetSubmissionDetailsAsync(string submissionId, string? userId = null)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null)
        {
            return null;
        }

        // If userId is provided, check if user owns this submission
        if (!string.IsNullOrEmpty(userId) && submission.UserId != userId)
        {
            return null;
        }

        var exam = await _examRepository.GetByIdAsync(submission.ExamId);
        if (exam == null)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(submission.UserId);

        // Get all questions for the exam
        var questionIds = submission.Answers.Select(a => a.QuestionId).ToList();
        var questions = (await _questionRepository.GetByIdsAsync(questionIds)).ToList();

        // Map submission answers to detailed question results
        var questionResults = new List<QuestionResultDto>();
        
        foreach (var answer in submission.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            questionResults.Add(new QuestionResultDto
            {
                Id = question.Id,
                Content = question.Content,
                Options = question.Options,
                CorrectAnswers = question.CorrectAnswers,
                UserAnswers = answer.SelectedAnswers,
                IsCorrect = answer.IsCorrect,
                Score = answer.Score,
                Explanation = question.Explanation
            });
        }

        return new SubmissionDto
        {
            Id = submission.Id,
            UserId = submission.UserId,
            Username = user?.Username ?? "Unknown",
            UserEmail = user?.Email,
            ExamId = submission.ExamId,
            ExamTitle = exam.Title,
            TotalScore = submission.TotalScore,
            MaxScore = exam.TotalScore,
            Questions = questionResults,
            SubmittedAt = submission.SubmittedAt,
            TimeTaken = submission.TimeTaken
        };
    }

    /// <summary>
    /// Maps a Submission entity to ExamHistoryDto.
    /// Encapsulates the transformation logic.
    /// </summary>
    private async Task<ExamHistoryDto?> MapToHistoryDtoAsync(Domain.Entities.Submission submission)
    {
        var exam = await _examRepository.GetByIdAsync(submission.ExamId);
        var user = await _userRepository.GetByIdAsync(submission.UserId);

        if (exam == null || user == null)
        {
            return null;
        }

        var correctAnswers = submission.Answers.Count(a => a.IsCorrect);
        var totalQuestions = submission.Answers.Count;
        var percentage = exam.TotalScore > 0 
            ? Math.Round((double)(submission.TotalScore / exam.TotalScore * 100), 2) 
            : 0;

        return new ExamHistoryDto
        {
            SubmissionId = submission.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            UserId = user.Id,
            Username = user.Username,
            UserEmail = user.Email,
            TotalScore = submission.TotalScore,
            MaxScore = exam.TotalScore,
            Percentage = percentage,
            TimeTaken = submission.TimeTaken,
            Duration = exam.Duration,
            SubmittedAt = submission.SubmittedAt,
            CorrectAnswers = correctAnswers,
            TotalQuestions = totalQuestions
        };
    }
}
