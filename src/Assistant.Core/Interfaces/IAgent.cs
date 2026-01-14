using Assistant.Core.Enums;

namespace Assistant.Core.Interfaces;

public interface IAgent
{
    AgentType Type { get; }
    Task<bool> CanHandleAsync(string input);
    Task<string> ProcessAsync(string input, Dictionary<string, object>? context = null);
}
