using Microsoft.Extensions.Options;
using QuizPlatform.Application.DTOs.Admin;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.ValueObjects;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizPlatform.Application.Services;

/// <summary>
/// Groq AI configuration options.
/// </summary>
public class GroqOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
}

/// <summary>
/// Service for AI-powered question extraction using Groq API.
/// </summary>
public class GroqAiService : IAiQuestionService
{
    private readonly HttpClient _httpClient;
    private readonly GroqOptions _options;
    private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqAiService(HttpClient httpClient, IOptions<GroqOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
    }

    /// <summary>
    /// Uses AI to extract quiz questions from document text.
    /// </summary>
    public async Task<IEnumerable<QuestionRequest>> ExtractQuestionsAsync(string documentText)
    {
        var prompt = BuildPrompt(documentText);

        var request = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 4096
        };

        var response = await _httpClient.PostAsJsonAsync(GroqApiUrl, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>();
        var content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "[]";

        return ParseQuestionsFromJson(content);
    }

    private string GetSystemPrompt()
    {
        return @"You are an expert at extracting multiple-choice quiz questions from educational documents.
Your task is to analyze the provided text and extract ALL quiz questions found.

For each question, identify:
1. The question content
2. All answer options (A, B, C, D, etc.)
3. The correct answer(s)
4. An explanation if provided, or generate one
5. The category/topic (infer from context)
6. Difficulty level (Easy, Medium, Hard)

IMPORTANT:
- Extract questions EXACTLY as they appear in the document
- Identify correct answers based on answer keys, highlighted text, or contextual clues
- If no clear correct answer is marked, use your knowledge to determine the correct answer
- Always return valid JSON array format

Return ONLY a JSON array with no additional text.";
    }

    private string BuildPrompt(string documentText)
    {
        return $@"Extract all multiple-choice questions from this document and return them as a JSON array.

Each question object must have this exact structure:
{{
    ""content"": ""The question text"",
    ""type"": ""Single"" or ""Multiple"",
    ""options"": [
        {{ ""key"": ""A"", ""content"": ""Option A text"" }},
        {{ ""key"": ""B"", ""content"": ""Option B text"" }},
        {{ ""key"": ""C"", ""content"": ""Option C text"" }},
        {{ ""key"": ""D"", ""content"": ""Option D text"" }}
    ],
    ""correctAnswers"": [""A""],
    ""explanation"": ""Why this answer is correct"",
    ""category"": ""Topic category"",
    ""difficulty"": ""Easy"", ""Medium"", or ""Hard""
}}

Document content:
---
{documentText}
---

Return ONLY the JSON array, no other text:";
    }

    private IEnumerable<QuestionRequest> ParseQuestionsFromJson(string jsonContent)
    {
        try
        {
            // Clean up the response - sometimes AI adds markdown code blocks
            jsonContent = jsonContent.Trim();
            if (jsonContent.StartsWith("```json"))
            {
                jsonContent = jsonContent.Substring(7);
            }
            if (jsonContent.StartsWith("```"))
            {
                jsonContent = jsonContent.Substring(3);
            }
            if (jsonContent.EndsWith("```"))
            {
                jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
            }
            jsonContent = jsonContent.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var questions = JsonSerializer.Deserialize<List<AiQuestionDto>>(jsonContent, options);
            
            return questions?.Select(q => new QuestionRequest
            {
                Content = q.Content ?? "",
                Type = q.Type ?? "Single",
                Options = q.Options?.Select(o => new AnswerOption
                {
                    Key = o.Key ?? "",
                    Content = o.Content ?? ""
                }).ToList() ?? new List<AnswerOption>(),
                CorrectAnswers = q.CorrectAnswers ?? new List<string>(),
                Explanation = q.Explanation,
                Category = q.Category ?? "General",
                Difficulty = q.Difficulty ?? "Medium"
            }) ?? Enumerable.Empty<QuestionRequest>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse AI response as JSON: {ex.Message}. Raw content: {jsonContent.Substring(0, Math.Min(500, jsonContent.Length))}");
        }
    }
}

#region DTO Classes for JSON Deserialization

internal class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}

internal class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}

internal class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

internal class AiQuestionDto
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("options")]
    public List<AiOptionDto>? Options { get; set; }

    [JsonPropertyName("correctAnswers")]
    public List<string>? CorrectAnswers { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; }
}

internal class AiOptionDto
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

#endregion
