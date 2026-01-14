using Assistant.Core.Enums;
using Assistant.Core.Interfaces;

namespace Assistant.Infrastructure.Agents;

public class CommandRouterAgent : IAgent
{
    private readonly IAIService _aiService;
    private readonly IEnumerable<IAgent> _agents;

    public AgentType Type => AgentType.CommandRouter;

    public CommandRouterAgent(IAIService aiService, IEnumerable<IAgent> agents)
    {
        _aiService = aiService;
        _agents = agents.Where(a => a.Type != AgentType.CommandRouter);
    }

    public Task<bool> CanHandleAsync(string input)
    {
        // Router всегда может обработать запрос
        return Task.FromResult(true);
    }

    public async Task<string> ProcessAsync(string input, Dictionary<string, object>? context = null)
    {
        // Определяем какой агент должен обработать запрос
        var agent = await DetermineAgentAsync(input);
        
        if (agent != null)
        {
            return await agent.ProcessAsync(input, context);
        }

        // Если не нашли подходящего агента, используем Query Agent (default)
        var queryAgent = _agents.FirstOrDefault(a => a.Type == AgentType.Query);
        if (queryAgent != null)
        {
            return await queryAgent.ProcessAsync(input, context);
        }

        return "Извините, я не могу обработать этот запрос.";
    }

    private async Task<IAgent?> DetermineAgentAsync(string input)
    {
        var systemPrompt = @"You are an intent classifier. Analyze the user's input and determine which agent should handle it.

Available agents:
- Task: for creating, updating, listing, or managing tasks/todos
- Reminder: for setting reminders, alarms, or scheduled notifications
- Memory: for searching past conversations or remembering information
- Query: for general questions, explanations, or information requests

Examples:
'добавь задачу купить молоко' -> Task
'напомни мне через час' -> Reminder
'что мы обсуждали вчера?' -> Memory
'объясни что такое REST API' -> Query

Respond ONLY with the agent name (Task, Reminder, Memory, or Query).";

        try
        {
            var response = await _aiService.GenerateResponseAsync(input, systemPrompt);
            var agentName = response.Trim().ToLower();

            return agentName switch
            {
                "task" => _agents.FirstOrDefault(a => a.Type == AgentType.Task),
                "reminder" => _agents.FirstOrDefault(a => a.Type == AgentType.Reminder),
                "memory" => _agents.FirstOrDefault(a => a.Type == AgentType.Memory),
                "query" => _agents.FirstOrDefault(a => a.Type == AgentType.Query),
                _ => null
            };
        }
        catch
        {
            // В случае ошибки возвращаем null (будет использован Query Agent)
            return null;
        }
    }
}
