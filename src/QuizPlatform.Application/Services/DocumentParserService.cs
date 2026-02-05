using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuizPlatform.Application.Interfaces;
using UglyToad.PdfPig;
using System.Text;

namespace QuizPlatform.Application.Services;

/// <summary>
/// Service for parsing PDF and DOCX documents to extract text.
/// </summary>
public class DocumentParserService : IDocumentParserService
{
    /// <summary>
    /// Parses content from a document file.
    /// </summary>
    public async Task<string> ParseDocumentAsync(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await ParsePdfAsync(fileStream),
            ".docx" => await ParseDocxAsync(fileStream),
            ".doc" => throw new NotSupportedException("Legacy .doc format is not supported. Please convert to .docx"),
            _ => throw new NotSupportedException($"File format '{extension}' is not supported. Use PDF or DOCX.")
        };
    }

    private Task<string> ParsePdfAsync(Stream fileStream)
    {
        var sb = new StringBuilder();
        
        using var document = PdfDocument.Open(fileStream);
        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return Task.FromResult(sb.ToString());
    }

    private Task<string> ParseDocxAsync(Stream fileStream)
    {
        var sb = new StringBuilder();
        
        using var document = WordprocessingDocument.Open(fileStream, false);
        var body = document.MainDocumentPart?.Document?.Body;
        
        if (body != null)
        {
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                sb.AppendLine(paragraph.InnerText);
            }
        }

        return Task.FromResult(sb.ToString());
    }
}
