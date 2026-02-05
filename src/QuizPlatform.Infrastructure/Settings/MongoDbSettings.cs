namespace QuizPlatform.Infrastructure.Settings;

/// <summary>
/// MongoDB connection settings loaded from configuration.
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}
