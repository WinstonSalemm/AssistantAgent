using Assistant.Core.Interfaces;
using Assistant.Infrastructure.AI;
using Assistant.Infrastructure.Agents;
using Assistant.Infrastructure.Data;
using Assistant.Infrastructure.Repositories;
using Assistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AssistantDbContext>(options =>
        {
            // Пробуем несколько способов получить connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? configuration["ConnectionStrings:DefaultConnection"]
                ?? configuration["ConnectionStrings__DefaultConnection"]
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            
            // Fallback: собираем connection string из PG* переменных (Railway стандарт)
            // ВАЖНО: PG* переменные доступны только внутри Postgres сервиса!
            // Для API сервиса нужно использовать ${{Postgres.DATABASE_URL}}
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var host = Environment.GetEnvironmentVariable("PGHOST");
                var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
                var db = Environment.GetEnvironmentVariable("PGDATABASE");
                var user = Environment.GetEnvironmentVariable("PGUSER");
                var pass = Environment.GetEnvironmentVariable("PGPASSWORD");
                
                if (!string.IsNullOrWhiteSpace(host) && 
                    !string.IsNullOrWhiteSpace(db) && 
                    !string.IsNullOrWhiteSpace(user) && 
                    !string.IsNullOrWhiteSpace(pass))
                {
                    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Prefer;Trust Server Certificate=true;";
                }
            }
            
            // Также пробуем DATABASE_URL напрямую (Railway может его предоставить)
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (!string.IsNullOrWhiteSpace(dbUrl))
                {
                    connectionString = dbUrl;
                }
            }
            
            // Проверяем что connection string не пустой
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var errorMsg = "ConnectionStrings:DefaultConnection is not configured and PG* variables are not available. " +
                    "Please set ConnectionStrings__DefaultConnection environment variable in Railway or ensure PG* variables are set. " +
                    $"Tried: GetConnectionString('DefaultConnection'), " +
                    $"configuration['ConnectionStrings:DefaultConnection'], " +
                    $"configuration['ConnectionStrings__DefaultConnection'], " +
                    $"Environment['ConnectionStrings__DefaultConnection'], " +
                    $"PGHOST={Environment.GetEnvironmentVariable("PGHOST")}, " +
                    $"PGDATABASE={Environment.GetEnvironmentVariable("PGDATABASE")}";
                throw new InvalidOperationException(errorMsg);
            }
            
            // Railway может предоставить DATABASE_URL в формате postgresql://
            // ВСЕГДА добавляем SSL параметры для Railway PostgreSQL
            var finalConnectionString = connectionString;
            
            // Если connection string в формате postgresql://, добавляем SSL параметры
            if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                // Проверяем есть ли уже параметры
                var hasParams = connectionString.Contains("?");
                var separator = hasParams ? "&" : "?";
                
                // ВСЕГДА добавляем SSL параметры для Railway
                if (!connectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    finalConnectionString = $"{connectionString}{separator}sslmode=Require";
                    separator = "&"; // Теперь точно есть параметры
                }
                
                // Добавляем Trust Server Certificate если его нет
                if (!finalConnectionString.Contains("Trust", StringComparison.OrdinalIgnoreCase) 
                    && !finalConnectionString.Contains("trust", StringComparison.OrdinalIgnoreCase))
                {
                    finalConnectionString = $"{finalConnectionString}&Trust Server Certificate=true";
                }
            }
            // Если connection string в формате Host=... (Npgsql формат), добавляем SSL параметры
            else if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                if (!connectionString.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase) 
                    && !connectionString.Contains("SslMode", StringComparison.OrdinalIgnoreCase)
                    && !connectionString.Contains("ssl mode", StringComparison.OrdinalIgnoreCase))
                {
                    finalConnectionString = $"{connectionString.TrimEnd(';')};SSL Mode=Require;Trust Server Certificate=true;";
                }
            }
            
            try
            {
                options.UseNpgsql(finalConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to configure PostgreSQL connection. " +
                    $"Original: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}... " +
                    $"Final: {finalPreview} " +
                    $"Has SSL: {hasSsl} " +
                    $"Error: {ex.Message}", ex);
            }
        });

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();

        // AI Service
        var openAIKey = configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(openAIKey))
        {
            services.AddSingleton<IAIService>(sp =>
            {
                var options = new OpenAIServiceOptions
                {
                    ChatModel = configuration["OpenAI:Model"] ?? "gpt-5-mini",
                    DeepThinkingModel = configuration["OpenAI:DeepThinkingModel"] ?? "gpt-5.2",
                    WhisperModel = configuration["OpenAI:WhisperModel"] ?? "whisper-1",
                    TTSModel = configuration["OpenAI:TTSModel"] ?? "tts-1",
                    EmbeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002"
                };
                return new OpenAIService(openAIKey, options);
            });
        }

        // Cache Service (Redis)
        var redisConnection = configuration.GetConnectionString("Redis");
        services.AddSingleton(sp => new CacheService(redisConnection));

        // AI Agents
        services.AddScoped<CommandRouterAgent>();
        services.AddScoped<IAgent, TaskAgent>();
        services.AddScoped<IAgent, QueryAgent>();
        
        services.AddScoped<IAgentRouter, AgentRouter>();

        return services;
    }
}
