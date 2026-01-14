namespace Assistant.Core.Interfaces;

public interface IAgentRouter
{
    Task<IAgent> RouteAsync(string input);
    Task<string> ProcessRequestAsync(string input, Dictionary<string, object>? context = null);
}
