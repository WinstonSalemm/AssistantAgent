namespace Assistant.Shared.DTOs;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public bool IsVoice { get; set; } = false;
}
