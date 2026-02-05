using QuizPlatform.Application.DTOs.Exam;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service interface for exam history operations.
/// Handles retrieval of completed exam submissions.
/// </summary>
public interface IExamHistoryService
{
    /// <summary>
    /// Get exam history for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated exam history</returns>
    Task<ExamHistoryListDto> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Get all exam history (admin only).
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="examId">Optional filter by exam ID</param>
    /// <param name="userId">Optional filter by user ID</param>
    /// <returns>Paginated exam history</returns>
    Task<ExamHistoryListDto> GetAllHistoryAsync(int page = 1, int pageSize = 20, string? examId = null, string? userId = null);

    /// <summary>
    /// Get detailed submission info by ID.
    /// </summary>
    /// <param name="submissionId">Submission ID</param>
    /// <param name="userId">Optional user ID for permission check</param>
    /// <returns>Submission details or null if not found/unauthorized</returns>
    Task<SubmissionDto?> GetSubmissionDetailsAsync(string submissionId, string? userId = null);
}
