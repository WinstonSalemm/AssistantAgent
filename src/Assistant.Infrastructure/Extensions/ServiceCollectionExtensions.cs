using Assistant.Core.Interfaces;
using Assistant.Infrastructure.AI;
using Assistant.Infrastructure.Agents;
using Assistant.Infrastructure.Data;
using Assistant.Infrastructure.Repositories;
using Assistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;

namespace Assistant.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // =========================
        // DATABASE (PostgreSQL)
        // =========================
        services.AddDbContext<AssistantDbContext>(options =>
        {
            string? rawConnectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? configuration["ConnectionStrings:DefaultConnection"]
                ?? configuration["ConnectionStrings__DefaultConnection"]
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            // Fallback: Railway PG* variables
            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                var host = Environment.GetEnvironmentVariable("PGHOST");
                var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
                var db   = Environment.GetEnvironmentVariable("PGDATABASE");
                var user = Environment.GetEnvironmentVariable("PGUSER");
                var pass = Environment.GetEnvironmentVariable("PGPASSWORD");

                if (!string.IsNullOrWhiteSpace(host)
                    && !string.IsNullOrWhiteSpace(db)
                    && !string.IsNullOrWhiteSpace(user)
                    && !string.IsNullOrWhiteSpace(pass))
                {
                    rawConnectionString =
                        $"Host={host};Port={port};Database={db};Username={user};Password={pass};";
                }
            }

            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                throw new InvalidOperationException(
                    "PostgreSQL connection string is not configured. " +
                    "Set ConnectionStrings__DefaultConnection or Railway PG* variables."
                );
            }

            // =========================
            // NORMALIZE CONNECTION STRING
            // =========================
            var builder = rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
                ? new NpgsqlConnectionStringBuilder(rawConnectionString)
                : new NpgsqlConnectionStringBuilder(rawConnectionString);

            var hostLower = builder.Host?.ToLowerInvariant() ?? "";

            bool isInternalRailway =
                hostLower.Contains("railway.internal");

            bool isProxyRailway =
                hostLower.Contains("proxy.rlwy.net")
                || hostLower.Contains("rlwy.net");

            // SSL rules (ВАЖНО)
            if (isInternalRailway)
            {
                // Internal network — SSL НЕ НУЖЕН
                builder.SslMode = SslMode.Disable;
            }
            else if (isProxyRailway)
            {
                // Public proxy — SSL обязателен
                builder.SslMode = SslMode.Require;
            }
            else
            {
                // Универсальный безопасный вариант
                builder.SslMode = SslMode.Prefer;
            }

            builder.Timeout = 15;
            builder.CommandTimeout = 30;
            builder.Pooling = true;

            var finalConnectionString = builder.ConnectionString;

            options.UseNpgsql(finalConnectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null
                );
            });
        });

        // =========================
        // REPOSITORIES
        // =========================
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();

        // =========================
        // AI SERVICE (OpenAI)
        // =========================
        var openAIKey = configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(openAIKey))
        {
            services.AddSingleton<IAIService>(_ =>
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

        // =========================
        // CACHE (Redis)
        // =========================
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton(_ => new CacheService(redisConnection));
        }

        // =========================
        // AI AGENTS
        // =========================
        services.AddScoped<CommandRouterAgent>();
        services.AddScoped<IAgent, TaskAgent>();
        services.AddScoped<IAgent, QueryAgent>();
        services.AddScoped<IAgentRouter, AgentRouter>();

        return services;
    }
}
