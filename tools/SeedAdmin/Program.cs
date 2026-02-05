using MongoDB.Driver;
using MongoDB.Bson;

// MongoDB connection
var connectionString = "mongodb+srv://tsmdoubleliff_db_user:EthernalStrife12@cluster0.hbpgiqg.mongodb.net/";
var databaseName = "english_quiz_db";

Console.WriteLine("Connecting to MongoDB...");

var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);
var usersCollection = database.GetCollection<BsonDocument>("users");
var questionsCollection = database.GetCollection<BsonDocument>("questions");
var examsCollection = database.GetCollection<BsonDocument>("exams");

// ========================================
// 1. Create Admin User
// ========================================
var filter = Builders<BsonDocument>.Filter.Eq("Username", "phuong123");
var existingAdmin = await usersCollection.Find(filter).FirstOrDefaultAsync();
var adminUserId = "";

if (existingAdmin != null)
{
    Console.WriteLine("✓ Admin user 'phuong123' already exists!");
    adminUserId = existingAdmin["_id"].AsString;
}
else
{
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("phuong123");
    adminUserId = Guid.NewGuid().ToString();
    
    var adminUser = new BsonDocument
    {
        { "_id", adminUserId },
        { "Username", "phuong123" },
        { "Email", "phuong123@admin.com" },
        { "PasswordHash", hashedPassword },
        { "Role", 1 },
        { "IsActive", true },
        { "CreatedAt", DateTime.UtcNow },
        { "UpdatedAt", DateTime.UtcNow }
    };
    
    await usersCollection.InsertOneAsync(adminUser);
    Console.WriteLine("✓ Admin user created!");
}

// ========================================
// 2. Create Sample Questions
// ========================================
var existingQuestions = await questionsCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
if (existingQuestions > 0)
{
    Console.WriteLine($"✓ {existingQuestions} questions already exist in database.");
}
else
{
    var questionIds = new List<string>();
    
    var sampleQuestions = new[]
    {
        ("What is the correct form: 'She ___ to school every day.'?", new[] { ("A", "go"), ("B", "goes"), ("C", "going"), ("D", "gone") }, "B", "Grammar"),
        ("Choose the synonym of 'happy':", new[] { ("A", "sad"), ("B", "angry"), ("C", "joyful"), ("D", "tired") }, "C", "Vocabulary"),
        ("Which sentence is correct?", new[] { ("A", "He don't like pizza."), ("B", "He doesn't likes pizza."), ("C", "He doesn't like pizza."), ("D", "He not like pizza.") }, "C", "Grammar"),
        ("Past tense of 'write':", new[] { ("A", "writed"), ("B", "wrote"), ("C", "written"), ("D", "writing") }, "B", "Grammar"),
        ("'I am interested ___ learning English.'", new[] { ("A", "on"), ("B", "at"), ("C", "in"), ("D", "for") }, "C", "Grammar"),
    };
    
    Console.WriteLine("Creating sample questions...");
    
    foreach (var (content, opts, answer, category) in sampleQuestions)
    {
        var questionId = Guid.NewGuid().ToString();
        questionIds.Add(questionId);
        
        var options = new BsonArray();
        foreach (var (key, text) in opts)
        {
            options.Add(new BsonDocument { { "Key", key }, { "Content", text } });
        }
        
        var question = new BsonDocument
        {
            { "_id", questionId },
            { "Content", content },
            { "Type", 0 },
            { "Options", options },
            { "CorrectAnswers", new BsonArray { answer } },
            { "Explanation", $"The correct answer is {answer}." },
            { "Score", 2.0 },
            { "Order", 0 },
            { "Category", category },
            { "Difficulty", 0 },
            { "CreatedAt", DateTime.UtcNow },
            { "UpdatedAt", DateTime.UtcNow }
        };
        
        await questionsCollection.InsertOneAsync(question);
    }
    Console.WriteLine($"✓ Created {questionIds.Count} sample questions!");
    
    // Create exam with these questions
    var examId = Guid.NewGuid().ToString();
    var exam = new BsonDocument
    {
        { "_id", examId },
        { "Title", "English Grammar Quiz - Beginner" },
        { "Description", "Test your basic English grammar knowledge." },
        { "Duration", 30 },
        { "TotalScore", 10.0 },
        { "IsActive", true },
        { "IsRandom", false },
        { "QuestionCount", 5 },
        { "QuestionIds", new BsonArray(questionIds) },
        { "Categories", new BsonArray { "Grammar", "Vocabulary" } },
        { "CreatedBy", adminUserId },
        { "CreatedAt", DateTime.UtcNow },
        { "UpdatedAt", DateTime.UtcNow }
    };
    
    await examsCollection.InsertOneAsync(exam);
    Console.WriteLine($"✓ Created exam: English Grammar Quiz - Beginner");
}

Console.WriteLine("\n========================================");
Console.WriteLine("Login: phuong123 / phuong123");
Console.WriteLine("========================================");

