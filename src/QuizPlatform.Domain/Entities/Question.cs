using QuizPlatform.Domain.Common;
using QuizPlatform.Domain.Enums;
using QuizPlatform.Domain.ValueObjects;

namespace QuizPlatform.Domain.Entities;

/// <summary>
/// Represents a question in the question bank or an exam.
/// </summary>
public class Question : BaseEntity
{
    public string? ExamId { get; set; } // null for question bank
    public string Content { get; set; } = null!; // Rich text HTML
    public QuestionType Type { get; set; } = QuestionType.Single;
    public List<AnswerOption> Options { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new(); // ["A"] or ["A", "C"]
    public string? Explanation { get; set; }
    public decimal Score { get; set; } = 1;
    public int Order { get; set; }
    public string? AudioUrl { get; set; } // For listening questions
    public string? ImageUrl { get; set; }
    public string? Category { get; set; } // grammar, vocabulary, etc.
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
}
