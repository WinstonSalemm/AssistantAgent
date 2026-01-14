using Assistant.Core.Interfaces;
using Assistant.Infrastructure.AI;
using Assistant.Infrastructure.Agents;
using Assistant.Infrastructure.Data;
using Assistant.Infrastructure.Repositories;
using Assistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            });
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
                    ChatModel = configuration["OpenAI:Model"] ?? "gpt-4o-mini",
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
