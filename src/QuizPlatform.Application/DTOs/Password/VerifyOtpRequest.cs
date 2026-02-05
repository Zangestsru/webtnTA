namespace QuizPlatform.Application.DTOs.Password;

/// <summary>
/// Request DTO for verifying OTP code.
/// </summary>
public class VerifyOtpRequest
{
    public string Email { get; set; } = null!;
    public string Otp { get; set; } = null!;
}
