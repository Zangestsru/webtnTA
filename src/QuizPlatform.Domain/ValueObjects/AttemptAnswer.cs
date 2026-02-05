namespace QuizPlatform.Domain.ValueObjects;

/// <summary>
/// Represents an in-progress answer during exam attempt.
/// </summary>
public class AttemptAnswer
{
    public string QuestionId { get; set; } = null!;
    public List<string> SelectedAnswers { get; set; } = new();
    public bool MarkedForReview { get; set; }
}
