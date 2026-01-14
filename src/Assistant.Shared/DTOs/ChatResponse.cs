namespace Assistant.Shared.DTOs;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public byte[]? AudioData { get; set; }
}
