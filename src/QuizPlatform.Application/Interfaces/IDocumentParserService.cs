namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service for parsing document files (PDF, DOCX) to extract text.
/// </summary>
public interface IDocumentParserService
{
    /// <summary>
    /// Parses content from a document file.
    /// </summary>
    /// <param name="fileStream">The file stream.</param>
    /// <param name="fileName">The original file name (for format detection).</param>
    /// <returns>The extracted text content.</returns>
    Task<string> ParseDocumentAsync(Stream fileStream, string fileName);
}
