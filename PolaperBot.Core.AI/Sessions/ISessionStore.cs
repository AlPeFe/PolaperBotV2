using Microsoft.Agents.AI;

namespace PolaperBot.Core.AI.Sessions;

public interface ISessionStore
{
    Task<AgentSession> LoadOrCreateAsync(long userId, CancellationToken cancellationToken = default);
    Task SaveAsync(long userId, AgentSession session, SessionUsage? usage = null, CancellationToken cancellationToken = default);
}
