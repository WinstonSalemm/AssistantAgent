namespace Assistant.Core.Interfaces;

public interface IVectorStore
{
    Task StoreAsync(string id, float[] embedding, Dictionary<string, string>? metadata = null);
    Task<IEnumerable<(string id, float similarity)>> SearchAsync(float[] query, int limit = 10);
}
