namespace QuizPlatform.Application.DTOs.Profile;

/// <summary>
/// Request DTO for changing user password.
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
