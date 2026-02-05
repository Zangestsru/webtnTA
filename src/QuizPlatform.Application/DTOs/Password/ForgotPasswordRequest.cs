namespace QuizPlatform.Application.DTOs.Password;

/// <summary>
/// Request DTO for forgot password - initiates OTP send.
/// </summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = null!;
}
