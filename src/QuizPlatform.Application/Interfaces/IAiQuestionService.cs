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

    /// <summary>
    /// Generates quiz questions based on document context and/or user prompt.
    /// </summary>
    /// <param name="documentText">Optional text context from a file.</param>
    /// <param name="userPrompt">Optional user instructions (e.g., "Create 5 questions").</param>
    /// <returns>A list of generated questions.</returns>
    Task<IEnumerable<QuestionRequest>> GenerateQuestionsAsync(string? documentText, string? userPrompt);
}
