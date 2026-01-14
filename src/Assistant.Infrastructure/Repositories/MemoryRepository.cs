using Assistant.Core.Entities;
using Assistant.Core.Interfaces;
using Assistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace Assistant.Infrastructure.Repositories;

public class MemoryRepository : Repository<Memory>, IMemoryRepository
{
    public MemoryRepository(AssistantDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Memory>> SearchSimilarAsync(float[] embedding, int limit = 10)
    {
        var vector = new Vector(embedding);
        
        // Используем сырой SQL для vector similarity search
        // Сортируем по косинусному расстоянию (меньше = более похоже)
        return await _dbSet
            .FromSqlRaw(@"
                SELECT * FROM ""Memories""
                ORDER BY ""Embedding"" <=> {0}
                LIMIT {1}", 
                vector, limit)
            .ToListAsync();
    }

    public async Task<Memory> StoreWithEmbeddingAsync(string content, float[] embedding)
    {
        var memory = new Memory
        {
            Id = Guid.NewGuid(),
            Content = content,
            Embedding = embedding,
            CreatedAt = DateTime.UtcNow
        };

        return await AddAsync(memory);
    }
}
