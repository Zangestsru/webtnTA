namespace QuizPlatform.Application.DTOs.Auth;

/// <summary>
/// Response DTO for authentication.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// User information DTO.
/// </summary>
public class UserDto
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
