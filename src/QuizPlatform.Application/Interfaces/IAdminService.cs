using QuizPlatform.Application.DTOs.Admin;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service interface for admin operations.
/// </summary>
public interface IAdminService
{
    // Question operations
    Task<IEnumerable<AdminQuestionDto>> GetAllQuestionsAsync();
    Task<AdminQuestionDto?> GetQuestionByIdAsync(string id);
    Task<AdminQuestionDto> CreateQuestionAsync(QuestionRequest request);
    Task<AdminQuestionDto> UpdateQuestionAsync(string id, QuestionRequest request);
    Task DeleteQuestionAsync(string id);

    // Exam operations
    Task<IEnumerable<AdminExamDto>> GetAllExamsAsync();
    Task<AdminExamDto?> GetExamByIdAsync(string id);
    Task<AdminExamDto> CreateExamAsync(string userId, ExamRequest request);
    Task<AdminExamDto> UpdateExamAsync(string id, ExamRequest request);
    Task DeleteExamAsync(string id);

    // User operations
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto> UpdateUserRoleAsync(string id, UpdateRoleRequest request);
    Task<AdminUserDto> ToggleUserActiveAsync(string id);
    Task DeleteUserAsync(string id);
}
