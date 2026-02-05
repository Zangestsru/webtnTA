namespace QuizPlatform.Domain.Common;

/// <summary>
/// Base entity class with common properties for all MongoDB documents.
/// </summary>
public abstract class BaseEntity
{
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
