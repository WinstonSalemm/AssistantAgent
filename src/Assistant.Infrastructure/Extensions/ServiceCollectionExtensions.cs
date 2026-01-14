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
            // Npgsql понимает оба формата
            // Для Railway добавляем SSL параметры если их нет
            var finalConnectionString = connectionString;
            
            // Если connection string в формате postgresql:// и нет SSL параметров, добавляем
            if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) 
                && !connectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                // Добавляем sslmode=require для Railway
                var separator = connectionString.Contains("?") ? "&" : "?";
                finalConnectionString = $"{connectionString}{separator}sslmode=Require";
            }
            
            // SSL параметры добавлены автоматически если нужно
            
            try
            {
                options.UseNpgsql(finalConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                    // Принудительно включаем SSL для Railway если нужно
                    if (finalConnectionString.Contains("sslmode=Require", StringComparison.OrdinalIgnoreCase))
                    {
                        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to configure PostgreSQL connection. " +
                    $"Original connection string preview: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}... " +
                    $"Final connection string preview: {finalConnectionString.Substring(0, Math.Min(50, finalConnectionString.Length))}... " +
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
