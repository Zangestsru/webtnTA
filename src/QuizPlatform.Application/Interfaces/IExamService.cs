using QuizPlatform.Application.DTOs.Exam;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service interface for exam operations.
/// </summary>
public interface IExamService
{
    Task<IEnumerable<ExamListDto>> GetActiveExamsAsync();
    Task<StartExamResponse> StartExamAsync(string userId, string examId);
    Task SaveProgressAsync(string attemptId, SaveProgressRequest request);
    Task<ExamResultDto> SubmitExamAsync(string userId, string attemptId, SubmitExamRequest request);
    Task<ExamResultDto?> GetResultAsync(string submissionId);
    Task<IEnumerable<ExamResultDto>> GetUserHistoryAsync(string userId);
}
