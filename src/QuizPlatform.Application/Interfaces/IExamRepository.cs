using QuizPlatform.Domain.Entities;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Repository interface for Exam operations.
/// </summary>
public interface IExamRepository : IRepository<Exam>
{
    Task<IEnumerable<Exam>> GetActiveExamsAsync();
    Task<IEnumerable<Exam>> GetExamsByCreatorAsync(string userId);
}
