using QuizPlatform.Application.DTOs.Auth;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse?> GoogleLoginAsync(string googleToken);
    Task<UserDto?> GetCurrentUserAsync(string userId);
}
