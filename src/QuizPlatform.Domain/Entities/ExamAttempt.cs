using QuizPlatform.Domain.Common;
using QuizPlatform.Domain.Enums;
using QuizPlatform.Domain.ValueObjects;

namespace QuizPlatform.Domain.Entities;

/// <summary>
/// Represents an exam attempt for tracking progress and timeout.
/// </summary>
public class ExamAttempt : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string ExamId { get; set; } = null!;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiredAt { get; set; }
    public AttemptStatus Status { get; set; } = AttemptStatus.Doing;
    public List<AttemptAnswer> CurrentAnswers { get; set; } = new();
}
