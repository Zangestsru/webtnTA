using MongoDB.Driver;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Infrastructure.Data;

namespace QuizPlatform.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of Exam repository.
/// </summary>
public class ExamRepository : IExamRepository
{
    private readonly IMongoCollection<Exam> _collection;

    public ExamRepository(MongoDbContext context)
    {
        _collection = context.Exams;
    }

    public async Task<Exam?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Exam>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Exam> CreateAsync(Exam entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, Exam entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == id, entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Exam>> GetActiveExamsAsync()
    {
        return await _collection.Find(x => x.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetExamsByCreatorAsync(string userId)
    {
        return await _collection.Find(x => x.CreatedBy == userId).ToListAsync();
    }
}
