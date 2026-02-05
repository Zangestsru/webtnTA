namespace QuizPlatform.Application.DTOs.Password;

/// <summary>
/// Request DTO for resetting password after OTP verification.
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = null!;
    public string Otp { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
