namespace QuizPlatform.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user login.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
