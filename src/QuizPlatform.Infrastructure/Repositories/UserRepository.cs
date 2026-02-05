using MongoDB.Driver;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Infrastructure.Data;

namespace QuizPlatform.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of User repository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext context)
    {
        _collection = context.Users;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<User> CreateAsync(User entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(string id, User entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == id, entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(x => x.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _collection.Find(x => x.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        return await _collection.Find(x => x.GoogleId == googleId).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _collection.Find(x => x.Email == email).AnyAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _collection.Find(x => x.Username == username).AnyAsync();
    }
}
