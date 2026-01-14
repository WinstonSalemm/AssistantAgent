namespace Assistant.Core.Entities;

public class Memory
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }  // Массив чисел для cosine similarity в коде
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
