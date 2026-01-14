using Assistant.Core.Entities;
using Assistant.Core.Interfaces;
using Assistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assistant.Infrastructure.Repositories;

public class MemoryRepository : Repository<Memory>, IMemoryRepository
{
    public MemoryRepository(AssistantDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Memory>> SearchSimilarAsync(float[] embedding, int limit = 10)
    {
        // Загружаем все памяти с embeddings
        var memories = await _dbSet
            .Where(m => m.Embedding != null && m.Embedding.Length > 0)
            .ToListAsync();
        
        // Вычисляем cosine similarity в коде
        var results = memories
            .Select(m => new
            {
                Memory = m,
                Similarity = CosineSimilarity(embedding, m.Embedding!)
            })
            .Where(x => x.Similarity > 0) // Фильтруем только похожие
            .OrderByDescending(x => x.Similarity) // Больше = более похоже
            .Take(limit)
            .Select(x => x.Memory)
            .ToList();
        
        return results;
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

    /// <summary>
    /// Вычисляет cosine similarity между двумя векторами
    /// </summary>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            return 0f;
        
        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;
        
        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }
        
        magnitudeA = MathF.Sqrt(magnitudeA);
        magnitudeB = MathF.Sqrt(magnitudeB);
        
        if (magnitudeA == 0f || magnitudeB == 0f)
            return 0f;
        
        return dotProduct / (magnitudeA * magnitudeB);
    }
}
