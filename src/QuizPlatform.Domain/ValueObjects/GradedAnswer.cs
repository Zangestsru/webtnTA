namespace QuizPlatform.Domain.ValueObjects;

/// <summary>
/// Represents a graded answer in a submission.
/// </summary>
public class GradedAnswer
{
    public string QuestionId { get; set; } = null!;
    public List<string> SelectedAnswers { get; set; } = new();
    public bool IsCorrect { get; set; }
    public decimal Score { get; set; }
}
