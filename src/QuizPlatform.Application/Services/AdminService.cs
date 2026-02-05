using QuizPlatform.Application.DTOs.Admin;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Application.Services;

/// <summary>
/// Service for admin operations.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IExamRepository _examRepository;
    private readonly IUserRepository _userRepository;

    public AdminService(
        IQuestionRepository questionRepository,
        IExamRepository examRepository,
        IUserRepository userRepository)
    {
        _questionRepository = questionRepository;
        _examRepository = examRepository;
        _userRepository = userRepository;
    }

    #region Question Operations

    public async Task<IEnumerable<AdminQuestionDto>> GetAllQuestionsAsync()
    {
        var questions = await _questionRepository.GetAllAsync();
        return questions.Select(MapToAdminQuestionDto);
    }

    public async Task<AdminQuestionDto?> GetQuestionByIdAsync(string id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        return question == null ? null : MapToAdminQuestionDto(question);
    }

    public async Task<AdminQuestionDto> CreateQuestionAsync(QuestionRequest request)
    {
        var question = new Question
        {
            Id = Guid.NewGuid().ToString(),
            ExamId = request.ExamId,
            Content = request.Content,
            Type = Enum.Parse<QuestionType>(request.Type, true),
            Options = request.Options,
            CorrectAnswers = request.CorrectAnswers,
            Explanation = request.Explanation,
            Score = request.Score,
            Order = request.Order,
            AudioUrl = request.AudioUrl,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            Difficulty = Enum.Parse<DifficultyLevel>(request.Difficulty, true),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _questionRepository.CreateAsync(question);
        return MapToAdminQuestionDto(question);
    }

    public async Task<AdminQuestionDto> UpdateQuestionAsync(string id, QuestionRequest request)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            throw new InvalidOperationException("Question not found");
        }

        question.ExamId = request.ExamId;
        question.Content = request.Content;
        question.Type = Enum.Parse<QuestionType>(request.Type, true);
        question.Options = request.Options;
        question.CorrectAnswers = request.CorrectAnswers;
        question.Explanation = request.Explanation;
        question.Score = request.Score;
        question.Order = request.Order;
        question.AudioUrl = request.AudioUrl;
        question.ImageUrl = request.ImageUrl;
        question.Category = request.Category;
        question.Difficulty = Enum.Parse<DifficultyLevel>(request.Difficulty, true);
        question.UpdatedAt = DateTime.UtcNow;

        await _questionRepository.UpdateAsync(id, question);
        return MapToAdminQuestionDto(question);
    }

    public async Task DeleteQuestionAsync(string id)
    {
        await _questionRepository.DeleteAsync(id);
    }

    #endregion

    #region Exam Operations

    public async Task<IEnumerable<AdminExamDto>> GetAllExamsAsync()
    {
        var exams = await _examRepository.GetAllAsync();
        return exams.Select(MapToAdminExamDto);
    }

    public async Task<AdminExamDto?> GetExamByIdAsync(string id)
    {
        var exam = await _examRepository.GetByIdAsync(id);
        return exam == null ? null : MapToAdminExamDto(exam);
    }

    public async Task<AdminExamDto> CreateExamAsync(string userId, ExamRequest request)
    {
        // Calculate total score from selected questions if not provided
        decimal totalScore = request.TotalScore;
        if (totalScore == 0 && request.QuestionIds != null && request.QuestionIds.Any())
        {
            var questions = await _questionRepository.GetByIdsAsync(request.QuestionIds);
            totalScore = questions.Sum(q => q.Score);
        }

        var exam = new Exam
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            Duration = request.Duration,
            TotalScore = totalScore,
            IsActive = request.IsActive,
            IsRandom = request.IsRandom,
            QuestionCount = request.QuestionCount,
            QuestionIds = request.QuestionIds ?? new List<string>(),
            Categories = request.Categories ?? new List<string>(),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _examRepository.CreateAsync(exam);
        return MapToAdminExamDto(exam);
    }

    public async Task<AdminExamDto> UpdateExamAsync(string id, ExamRequest request)
    {
        var exam = await _examRepository.GetByIdAsync(id);
        if (exam == null)
        {
            throw new InvalidOperationException("Exam not found");
        }

        // Calculate total score from selected questions if not provided
        decimal totalScore = request.TotalScore;
        if (totalScore == 0 && request.QuestionIds != null && request.QuestionIds.Any())
        {
            var questions = await _questionRepository.GetByIdsAsync(request.QuestionIds);
            totalScore = questions.Sum(q => q.Score);
        }

        exam.Title = request.Title;
        exam.Description = request.Description;
        exam.Duration = request.Duration;
        exam.TotalScore = totalScore;
        exam.IsActive = request.IsActive;
        exam.IsRandom = request.IsRandom;
        exam.QuestionCount = request.QuestionCount;
        exam.QuestionIds = request.QuestionIds ?? new List<string>();
        exam.Categories = request.Categories ?? new List<string>();
        exam.UpdatedAt = DateTime.UtcNow;

        await _examRepository.UpdateAsync(id, exam);
        return MapToAdminExamDto(exam);
    }

    public async Task DeleteExamAsync(string id)
    {
        await _examRepository.DeleteAsync(id);
    }

    #endregion

    #region User Operations

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToAdminUserDto);
    }

    public async Task<AdminUserDto> UpdateUserRoleAsync(string id, UpdateRoleRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Role = Enum.Parse<UserRole>(request.Role, true);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(id, user);
        return MapToAdminUserDto(user);
    }

    public async Task<AdminUserDto> ToggleUserActiveAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(id, user);
        return MapToAdminUserDto(user);
    }

    #endregion

    #region Mapping Methods

    private static AdminQuestionDto MapToAdminQuestionDto(Question q)
    {
        return new AdminQuestionDto
        {
            Id = q.Id,
            ExamId = q.ExamId,
            Content = q.Content,
            Type = q.Type.ToString(),
            Options = q.Options,
            CorrectAnswers = q.CorrectAnswers,
            Explanation = q.Explanation,
            Score = q.Score,
            Category = q.Category,
            Difficulty = q.Difficulty.ToString(),
            CreatedAt = q.CreatedAt
        };
    }

    private static AdminExamDto MapToAdminExamDto(Exam e)
    {
        return new AdminExamDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Duration = e.Duration,
            TotalScore = e.TotalScore,
            IsActive = e.IsActive,
            IsRandom = e.IsRandom,
            QuestionCount = e.QuestionCount,
            QuestionIds = e.QuestionIds ?? new List<string>(),
            Categories = e.Categories ?? new List<string>(),
            CreatedAt = e.CreatedAt
        };
    }

    private static AdminUserDto MapToAdminUserDto(User u)
    {
        return new AdminUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    #endregion
}
