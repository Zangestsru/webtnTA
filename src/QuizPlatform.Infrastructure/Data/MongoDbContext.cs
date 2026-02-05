using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Infrastructure.Settings;

namespace QuizPlatform.Infrastructure.Data;

/// <summary>
/// MongoDB database context for managing collections.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Exam> Exams => _database.GetCollection<Exam>("exams");
    public IMongoCollection<Question> Questions => _database.GetCollection<Question>("questions");
    public IMongoCollection<Submission> Submissions => _database.GetCollection<Submission>("submissions");
    public IMongoCollection<ExamAttempt> ExamAttempts => _database.GetCollection<ExamAttempt>("exam_attempts");
}
