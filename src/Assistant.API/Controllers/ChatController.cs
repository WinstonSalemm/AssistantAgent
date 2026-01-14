using Assistant.Core.Entities;
using Assistant.Core.Enums;
using Assistant.Core.Interfaces;
using Assistant.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAgentRouter _agentRouter;
    private readonly IAIService _aiService;
    private readonly IRepository<Message> _messageRepository;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IAgentRouter agentRouter,
        IAIService aiService,
        IRepository<Message> messageRepository,
        ILogger<ChatController> logger)
    {
        _agentRouter = agentRouter;
        _aiService = aiService;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            var sessionId = request.SessionId ?? Guid.NewGuid();

            // Сохраняем сообщение пользователя
            await _messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                Role = MessageRole.User,
                Content = request.Message,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            });

            // Обрабатываем запрос через агентов
            var response = await _agentRouter.ProcessRequestAsync(request.Message);

            // Сохраняем ответ ассистента
            await _messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                Role = MessageRole.Assistant,
                Content = response,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            });

            // Генерируем TTS если нужно
            byte[]? audioData = null;
            if (request.IsVoice)
            {
                try
                {
                    audioData = await _aiService.TextToSpeechAsync(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating TTS");
                }
            }

            return Ok(new ChatResponse
            {
                Message = response,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow,
                AudioData = audioData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("voice")]
    public async Task<ActionResult<ChatResponse>> SendVoiceMessage([FromForm] IFormFile audio)
    {
        try
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest("No audio file provided");
            }

            // Транскрибируем аудио
            string transcription;
            using (var stream = audio.OpenReadStream())
            {
                transcription = await _aiService.TranscribeAudioAsync(stream, audio.FileName);
            }

            // Обрабатываем как обычный текстовый запрос
            var chatRequest = new ChatRequest
            {
                Message = transcription,
                IsVoice = true
            };

            return await SendMessage(chatRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing voice message");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<Message>>> GetHistory([FromQuery] Guid? sessionId = null)
    {
        try
        {
            if (sessionId.HasValue)
            {
                var messages = await _messageRepository.FindAsync(m => m.SessionId == sessionId.Value);
                return Ok(messages.OrderBy(m => m.CreatedAt));
            }

            var allMessages = await _messageRepository.GetAllAsync();
            return Ok(allMessages.OrderByDescending(m => m.CreatedAt).Take(50));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history");
            return StatusCode(500, "Internal server error");
        }
    }
}
