using MongoDB.Driver;
using BCrypt.Net;

// MongoDB connection string from your appsettings.json
var connectionString = "mongodb+srv://tsmdoubleliff_db_user:EthernalStrife12@cluster0.hbpgiqg.mongodb.net/";
var databaseName = "english_quiz_db";

Console.WriteLine("Connecting to MongoDB...");

var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);
var usersCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("users");

// Check if admin already exists
var existingAdmin = await usersCollection.Find(
    new MongoDB.Bson.BsonDocument("username", "phuong123")
).FirstOrDefaultAsync();

if (existingAdmin != null)
{
    Console.WriteLine("Admin user 'phuong123' already exists!");
    return;
}

// Create admin user
var adminUser = new MongoDB.Bson.BsonDocument
{
    { "_id", Guid.NewGuid().ToString() },
    { "Username", "phuong123" },
    { "Email", "phuong123@admin.com" },
    { "PasswordHash", BCrypt.Net.BCrypt.HashPassword("phuong123") },
    { "Role", 1 }, // 1 = Admin enum value
    { "IsActive", true },
    { "CreatedAt", DateTime.UtcNow },
    { "UpdatedAt", DateTime.UtcNow }
};

await usersCollection.InsertOneAsync(adminUser);
Console.WriteLine("Admin user 'phuong123' created successfully!");
Console.WriteLine("Email: phuong123@admin.com");
Console.WriteLine("Password: phuong123");
Console.WriteLine("Role: Admin");
