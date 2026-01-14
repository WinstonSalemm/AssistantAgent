using Assistant.Core.Enums;
using Assistant.Core.Interfaces;

namespace Assistant.Infrastructure.Agents;

public class QueryAgent : IAgent
{
    private readonly IAIService _aiService;

    public AgentType Type => AgentType.Query;

    public QueryAgent(IAIService aiService)
    {
        _aiService = aiService;
    }

    public Task<bool> CanHandleAsync(string input)
    {
        // Query agent - default агент, всегда может обработать
        return Task.FromResult(true);
    }

    public async Task<string> ProcessAsync(string input, Dictionary<string, object>? context = null)
    {
        var systemPrompt = @"Ты - персональный AI-ассистент. Твоя задача - помогать пользователю отвечать на вопросы, объяснять концепции и давать полезные советы.

Правила:
- Отвечай кратко и по делу
- Используй простой язык
- Если не знаешь ответ - скажи честно
- Будь дружелюбным и helpful";

        try
        {
            var response = await _aiService.GenerateResponseAsync(input, systemPrompt);
            return response;
        }
        catch (Exception ex)
        {
            return $"Извините, произошла ошибка при обработке запроса: {ex.Message}";
        }
    }
}
