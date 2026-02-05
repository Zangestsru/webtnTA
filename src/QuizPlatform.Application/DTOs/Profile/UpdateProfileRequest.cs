using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Application.DTOs.Profile;

/// <summary>
/// Request DTO for updating user profile.
/// </summary>
public class UpdateProfileRequest
{
    public string? Username { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender? Gender { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
}
