using MongoDB.Driver;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;
using QuizPlatform.Infrastructure.Data;

namespace QuizPlatform.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of ExamAttempt repository.
/// </summary>
public class ExamAttemptRepository : IExamAttemptRepository
{
    private readonly IMongoCollection<ExamAttempt> _collection;

    public ExamAttemptRepository(MongoDbContext context)
    {
        _collection = context.ExamAttempts;
    }

    public async Task<ExamAttempt?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExamAttempt>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<ExamAttempt> CreateAsync(ExamAttempt entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, ExamAttempt entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == id, entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<ExamAttempt?> GetActiveAttemptAsync(string userId, string examId)
    {
        return await _collection.Find(x => 
            x.UserId == userId && 
            x.ExamId == examId && 
            x.Status == AttemptStatus.Doing)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExamAttempt>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExamAttempt>> GetByStatusAsync(AttemptStatus status)
    {
        return await _collection.Find(x => x.Status == status).ToListAsync();
    }
}
