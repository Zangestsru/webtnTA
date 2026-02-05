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

COPY --from=build /app/publish .

# Railway injects PORT environment variable
# Default to 8080 if not set
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "QuizPlatform.API.dll"]
