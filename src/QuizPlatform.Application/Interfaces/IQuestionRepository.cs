using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Repository interface for Question operations.
/// </summary>
public interface IQuestionRepository : IRepository<Question>
{
    Task<IEnumerable<Question>> GetByExamIdAsync(string examId);
    Task<IEnumerable<Question>> GetByIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<Question>> GetQuestionBankAsync();
    Task<IEnumerable<Question>> GetByCategoryAsync(string category);
    Task<IEnumerable<Question>> GetByDifficultyAsync(DifficultyLevel difficulty);
    Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count, string? category = null, DifficultyLevel? difficulty = null);
    Task<IEnumerable<Question>> GetRandomQuestionsByCategoriesAsync(int count, IEnumerable<string> categories);
}

