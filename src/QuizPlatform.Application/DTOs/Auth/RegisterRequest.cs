using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user registration.
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
