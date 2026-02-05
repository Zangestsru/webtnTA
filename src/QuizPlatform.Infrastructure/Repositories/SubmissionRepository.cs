using MongoDB.Driver;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Infrastructure.Data;

namespace QuizPlatform.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of Submission repository.
/// </summary>
public class SubmissionRepository : ISubmissionRepository
{
    private readonly IMongoCollection<Submission> _collection;

    public SubmissionRepository(MongoDbContext context)
    {
        _collection = context.Submissions;
    }

    public async Task<Submission?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Submission>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Submission> CreateAsync(Submission entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, Submission entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == id, entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Submission>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.SubmittedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Submission>> GetByExamIdAsync(string examId)
    {
        return await _collection.Find(x => x.ExamId == examId).ToListAsync();
    }

    public async Task<Submission?> GetByUserAndExamAsync(string userId, string examId)
    {
        return await _collection.Find(x => x.UserId == userId && x.ExamId == examId)
            .SortByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
    }
}
