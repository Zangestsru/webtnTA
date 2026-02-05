namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation.
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string email, string role);
}
