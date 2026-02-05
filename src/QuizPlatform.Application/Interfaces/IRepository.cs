using QuizPlatform.Domain.Common;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Generic repository interface for MongoDB operations.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
}
