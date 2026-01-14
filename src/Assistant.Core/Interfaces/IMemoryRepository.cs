using Assistant.Core.Entities;

namespace Assistant.Core.Interfaces;

public interface IMemoryRepository : IRepository<Memory>
{
    Task<IEnumerable<Memory>> SearchSimilarAsync(float[] embedding, int limit = 10);
    Task<Memory> StoreWithEmbeddingAsync(string content, float[] embedding);
}
