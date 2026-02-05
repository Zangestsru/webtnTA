using QuizPlatform.Domain.Entities;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Repository interface for User operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
}
