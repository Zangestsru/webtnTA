using MongoDB.Driver;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;
using QuizPlatform.Infrastructure.Data;

namespace QuizPlatform.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of Question repository.
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly IMongoCollection<Question> _collection;

    public QuestionRepository(MongoDbContext context)
    {
        _collection = context.Questions;
    }

    public async Task<Question?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Question> CreateAsync(Question entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, Question entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == id, entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Question>> GetByExamIdAsync(string examId)
    {
        return await _collection.Find(x => x.ExamId == examId).SortBy(x => x.Order).ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return Enumerable.Empty<Question>();
        
        var filter = Builders<Question>.Filter.In(x => x.Id, idList);
        var questions = await _collection.Find(filter).ToListAsync();
        
        // Preserve the order of IDs as provided
        return idList.Select(id => questions.FirstOrDefault(q => q.Id == id))
                     .Where(q => q != null)
                     .Cast<Question>();
    }

    public async Task<IEnumerable<Question>> GetQuestionBankAsync()
    {
        return await _collection.Find(x => x.ExamId == null).ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetByCategoryAsync(string category)
    {
        return await _collection.Find(x => x.Category == category).ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetByDifficultyAsync(DifficultyLevel difficulty)
    {
        return await _collection.Find(x => x.Difficulty == difficulty).ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count, string? category = null, DifficultyLevel? difficulty = null)
    {
        var filterBuilder = Builders<Question>.Filter;
        var filter = filterBuilder.Eq(x => x.ExamId, (string?)null);

        if (!string.IsNullOrEmpty(category))
        {
            filter &= filterBuilder.Eq(x => x.Category, category);
        }

        if (difficulty.HasValue)
        {
            filter &= filterBuilder.Eq(x => x.Difficulty, difficulty.Value);
        }

        // Use MongoDB aggregation with $sample stage for random selection
        var matchStage = new MongoDB.Bson.BsonDocument("$match", 
            new MongoDB.Bson.BsonDocument
            {
                { "ExamId", MongoDB.Bson.BsonNull.Value }
            });
        
        if (!string.IsNullOrEmpty(category))
        {
            matchStage["$match"]["Category"] = category;
        }
        
        if (difficulty.HasValue)
        {
            matchStage["$match"]["Difficulty"] = difficulty.Value.ToString();
        }

        var sampleStage = new MongoDB.Bson.BsonDocument("$sample", 
            new MongoDB.Bson.BsonDocument("size", count));

        var pipeline = new[] { matchStage, sampleStage };
        
        return await _collection.Aggregate<Question>(pipeline).ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetRandomQuestionsByCategoriesAsync(int count, IEnumerable<string> categories)
    {
        var categoryList = categories.ToList();
        
        var matchDoc = new MongoDB.Bson.BsonDocument
        {
            { "ExamId", MongoDB.Bson.BsonNull.Value }
        };
        
        // Filter by categories if provided
        if (categoryList.Any())
        {
            matchDoc["Category"] = new MongoDB.Bson.BsonDocument("$in", new MongoDB.Bson.BsonArray(categoryList));
        }

        var matchStage = new MongoDB.Bson.BsonDocument("$match", matchDoc);
        var sampleStage = new MongoDB.Bson.BsonDocument("$sample", new MongoDB.Bson.BsonDocument("size", count));

        var pipeline = new[] { matchStage, sampleStage };
        
        return await _collection.Aggregate<Question>(pipeline).ToListAsync();
    }
}
