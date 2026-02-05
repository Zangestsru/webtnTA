using QuizPlatform.Domain.Common;

namespace QuizPlatform.Domain.Entities;

/// <summary>
/// Represents an exam/quiz.
/// </summary>
public class Exam : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Duration { get; set; } // in minutes
    public decimal TotalScore { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsRandom { get; set; }
    public int QuestionCount { get; set; }
    public List<string> QuestionIds { get; set; } = new(); // Specific question IDs for manual selection
    public List<string> Categories { get; set; } = new(); // Categories for random selection
    public string CreatedBy { get; set; } = null!; // User ID
}

