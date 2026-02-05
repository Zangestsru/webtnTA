using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Application.Services;
using QuizPlatform.Application.Settings;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;
using QuizPlatform.Infrastructure.Data;
using QuizPlatform.Infrastructure.Repositories;
using QuizPlatform.Infrastructure.Services;
using QuizPlatform.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IExamAttemptRepository, ExamAttemptRepository>();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IExamHistoryService, ExamHistoryService>();

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Document Import Services
builder.Services.AddScoped<IDocumentParserService, DocumentParserService>();
builder.Services.AddHttpClient<IAiQuestionService, GroqAiService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? ""))
        };
    });

builder.Services.AddAuthorization();

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Anh ngữ Ephata API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(',') 
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Health check endpoint for Railway - MUST be first so it's always available
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Log startup info
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
logger.LogInformation("Starting application on port {Port}", port);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger in production for API documentation
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quiz Platform API v1");
        c.RoutePrefix = "swagger";
    });
}

// Don't use HTTPS redirection in development (use HTTP profile)
// In production, Railway/Azure handles SSL termination
// app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed admin user on startup (non-blocking)
try
{
    await SeedAdminUserAsync(app.Services);
}
catch (Exception ex)
{
    var seedLogger = app.Services.GetRequiredService<ILogger<Program>>();
    seedLogger.LogWarning(ex, "Failed to seed admin user during startup. Will retry on first request if needed.");
}

app.Run();

// ============================================
// Admin Seeder - Creates admin on first startup
// ============================================
static async Task SeedAdminUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // Get admin credentials from environment/config
    var adminUsername = config["Admin:Username"] ?? Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
    var adminEmail = config["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@quizplatform.com";
    var adminPassword = config["Admin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123456";

    try
    {
        // Check if admin already exists
        var existingAdmin = await userRepository.GetByUsernameAsync(adminUsername);
        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user '{Username}' already exists", adminUsername);
            return;
        }

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = adminUsername,
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.CreateAsync(adminUser);
        logger.LogInformation("✓ Admin user '{Username}' created successfully!", adminUsername);
        logger.LogWarning("⚠ Default admin password is being used. Please change it immediately!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed admin user");
    }
}
