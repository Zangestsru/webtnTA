using QuizPlatform.Domain.Entities;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Repository interface for Submission operations.
/// </summary>
public interface ISubmissionRepository : IRepository<Submission>
{
    Task<IEnumerable<Submission>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Submission>> GetByExamIdAsync(string examId);
    Task<Submission?> GetByUserAndExamAsync(string userId, string examId);
}
