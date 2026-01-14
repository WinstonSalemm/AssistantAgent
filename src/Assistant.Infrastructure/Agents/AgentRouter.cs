using Assistant.Core.Enums;
using Assistant.Core.Interfaces;

namespace Assistant.Infrastructure.Agents;

public class AgentRouter : IAgentRouter
{
    private readonly IEnumerable<IAgent> _agents;
    private readonly CommandRouterAgent _commandRouter;

    public AgentRouter(IEnumerable<IAgent> agents, CommandRouterAgent commandRouter)
    {
        _agents = agents;
        _commandRouter = commandRouter;
    }

    public async Task<IAgent> RouteAsync(string input)
    {
        // Пробуем найти агента который может обработать запрос
        foreach (var agent in _agents.Where(a => a.Type != AgentType.CommandRouter))
        {
            if (await agent.CanHandleAsync(input))
            {
                return agent;
            }
        }

        // Если не нашли - используем CommandRouter
        return _commandRouter;
    }

    public async Task<string> ProcessRequestAsync(string input, Dictionary<string, object>? context = null)
    {
        // Всегда используем CommandRouter для определения правильного агента
        return await _commandRouter.ProcessAsync(input, context);
    }
}
