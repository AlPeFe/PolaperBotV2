using Microsoft.Agents.AI;
using PolaperBot.Core.AI.Sessions;

namespace PolaperBot.Core.AI.Services;

public interface IAgentService
{
    Task<string> SendMessageAsync(long userId, string message, CancellationToken cancellationToken = default);
}

public class AgentService : IAgentService
{
    private readonly AIAgent _agent;
    private readonly ISessionStore _sessionStore;

    public AgentService(AIAgent agent, ISessionStore sessionStore)
    {
        _agent = agent;
        _sessionStore = sessionStore;
    }

    public async Task<string> SendMessageAsync(long userId, string message, CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.LoadOrCreateAsync(userId, cancellationToken);

        var response = await _agent.RunAsync(message, session, cancellationToken: cancellationToken);

        await _sessionStore.SaveAsync(userId, session, cancellationToken);

        return response?.ToString() ?? "No pude procesar tu mensaje.";
    }
}
