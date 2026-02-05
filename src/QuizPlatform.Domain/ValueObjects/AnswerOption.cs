namespace QuizPlatform.Domain.ValueObjects;

/// <summary>
/// Represents an answer option for a question.
/// </summary>
public class AnswerOption
{
    public string Key { get; set; } = null!; // A, B, C, D
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
}
