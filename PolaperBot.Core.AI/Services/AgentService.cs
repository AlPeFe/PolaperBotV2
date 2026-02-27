using System.Diagnostics;
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
    private readonly IUserContext _userContext;

    public AgentService(AIAgent agent, ISessionStore sessionStore, IUserContext userContext)
    {
        _agent = agent;
        _sessionStore = sessionStore;
        _userContext = userContext;
    }

    public async Task<string> SendMessageAsync(long userId, string message, CancellationToken cancellationToken = default)
    {
        _userContext.CurrentChatId = userId;
        
        var session = await _sessionStore.LoadOrCreateAsync(userId, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var response = await _agent.RunAsync(message, session, cancellationToken: cancellationToken);
        stopwatch.Stop();

        var usage = ExtractUsage(response, stopwatch.ElapsedMilliseconds);
        await _sessionStore.SaveAsync(userId, session, usage, cancellationToken);

        return response?.ToString() ?? "No pude procesar tu mensaje.";
    }

    private static SessionUsage ExtractUsage(AgentResponse? response, long processingTimeMs)
    {
        if (response == null)
            return new SessionUsage { ProcessingTimeMs = processingTimeMs, Timestamp = DateTime.UtcNow };

        var usageDetails = response.Usage;

        var usage = new SessionUsage
        {
            ProcessingTimeMs = processingTimeMs,
            Timestamp = DateTime.UtcNow,
            OutputTokens = Convert.ToInt32(usageDetails.OutputTokenCount),
            InputTokens = Convert.ToInt32(usageDetails.InputTokenCount)
        };

        return usage;
    }
}
