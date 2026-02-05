using QuizPlatform.Domain.Common;
using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public string? GoogleId { get; set; }
}
