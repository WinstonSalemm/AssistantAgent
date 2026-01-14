namespace Assistant.Core.Interfaces;

public interface IAIService
{
    Task<string> GenerateResponseAsync(string prompt, string? systemPrompt = null);
    Task<string> GenerateResponseAsync(string prompt, string model, string? systemPrompt = null);
    Task<string> TranscribeAudioAsync(Stream audioStream, string fileName);
    Task<byte[]> TextToSpeechAsync(string text);
    Task<float[]> GenerateEmbeddingAsync(string text);
}
