using Assistant.Core.Interfaces;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Audio;
using OpenAI.Embeddings;

namespace Assistant.Infrastructure.AI;

public class OpenAIService : IAIService
{
    private readonly OpenAIClient _client;
    private readonly string _chatModel;
    private readonly string _whisperModel;
    private readonly string _ttsModel;
    private readonly string _embeddingModel;

    public OpenAIService(string apiKey, OpenAIServiceOptions? options = null)
    {
        _client = new OpenAIClient(apiKey);
        
        var opts = options ?? new OpenAIServiceOptions();
        _chatModel = opts.ChatModel;
        _whisperModel = opts.WhisperModel;
        _ttsModel = opts.TTSModel;
        _embeddingModel = opts.EmbeddingModel;
    }

    public async Task<string> GenerateResponseAsync(string prompt, string? systemPrompt = null)
    {
        var chatClient = _client.GetChatClient(_chatModel);
        
        var messages = new List<ChatMessage>();
        
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new SystemChatMessage(systemPrompt));
        }
        
        messages.Add(new UserChatMessage(prompt));

        var completion = await chatClient.CompleteChatAsync(messages);
        
        return completion.Value.Content[0].Text;
    }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        var audioClient = _client.GetAudioClient(_whisperModel);
        
        var transcription = await audioClient.TranscribeAudioAsync(
            audioStream,
            fileName
        );

        return transcription.Value.Text;
    }

    public async Task<byte[]> TextToSpeechAsync(string text)
    {
        var audioClient = _client.GetAudioClient(_ttsModel);
        
        var speechResult = await audioClient.GenerateSpeechAsync(
            text,
            GeneratedSpeechVoice.Alloy
        );

        using var memoryStream = new MemoryStream();
        await speechResult.Value.ToStream().CopyToAsync(memoryStream);
        
        return memoryStream.ToArray();
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var embeddingClient = _client.GetEmbeddingClient(_embeddingModel);
        
        var embedding = await embeddingClient.GenerateEmbeddingAsync(text);
        
        return embedding.Value.ToFloats().ToArray();
    }
}

public class OpenAIServiceOptions
{
    public string ChatModel { get; set; } = "gpt-4o-mini";
    public string WhisperModel { get; set; } = "whisper-1";
    public string TTSModel { get; set; } = "tts-1";
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
}
