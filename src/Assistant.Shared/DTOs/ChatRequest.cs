namespace Assistant.Shared.DTOs;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public bool IsVoice { get; set; } = false;
    public bool UseDeepThinking { get; set; } = false; // Кнопка "думай глубже" → gpt-5.2
}
