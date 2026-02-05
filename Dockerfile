# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY src/QuizPlatform.Domain/QuizPlatform.Domain.csproj src/QuizPlatform.Domain/
COPY src/QuizPlatform.Application/QuizPlatform.Application.csproj src/QuizPlatform.Application/
COPY src/QuizPlatform.Infrastructure/QuizPlatform.Infrastructure.csproj src/QuizPlatform.Infrastructure/
COPY src/QuizPlatform.API/QuizPlatform.API.csproj src/QuizPlatform.API/
COPY QuizPlatform.sln .

RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /src/src/QuizPlatform.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

COPY --from=build /app/publish .

# Change ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Set production environment
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Use shell form to allow PORT environment variable substitution at runtime
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet QuizPlatform.API.dll
