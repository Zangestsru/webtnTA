using QuizPlatform.Application.DTOs.Admin;

namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service for AI-powered question extraction from text.
/// </summary>
public interface IAiQuestionService
{
    /// <summary>
    /// Uses AI to extract quiz questions from document text.
    /// </summary>
    /// <param name="documentText">The text extracted from a document.</param>
    /// <returns>A list of extracted questions ready to be saved.</returns>
    Task<IEnumerable<QuestionRequest>> ExtractQuestionsAsync(string documentText);
}
