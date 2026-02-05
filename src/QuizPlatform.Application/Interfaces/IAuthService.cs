using QuizPlatform.Application.DTOs.Auth;
using QuizPlatform.Application.DTOs.Password;
using QuizPlatform.Application.DTOs.Profile;

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
    
    // Password reset methods
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    
    // Profile management methods
    Task<UserDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
