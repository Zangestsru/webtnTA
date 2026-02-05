using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Repository interface for ExamAttempt operations.
/// </summary>
public interface IExamAttemptRepository : IRepository<ExamAttempt>
{
    Task<ExamAttempt?> GetActiveAttemptAsync(string userId, string examId);
    Task<IEnumerable<ExamAttempt>> GetByUserIdAsync(string userId);
    Task<IEnumerable<ExamAttempt>> GetByStatusAsync(AttemptStatus status);
}
