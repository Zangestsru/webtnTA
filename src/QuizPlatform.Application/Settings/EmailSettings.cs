namespace QuizPlatform.Application.Settings;

/// <summary>
/// Email configuration settings.
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = null!;
    public string SmtpPass { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
