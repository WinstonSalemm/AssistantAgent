namespace Assistant.Core.Entities;

public class Memory
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }  // Vector для pgvector
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
