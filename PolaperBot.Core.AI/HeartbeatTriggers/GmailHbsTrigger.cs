using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Services;

namespace PolaperBot.Core.AI.HeartbeatTriggers;

public class GmailHbsTrigger : IHeartbeatTrigger
{
    private readonly ILogger<GmailHbsTrigger> _logger;

    public string Name => "GmailHbs";

    public GmailHbsTrigger(ILogger<GmailHbsTrigger> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GmailHbs trigger executed - checking emails");
        
        return Task.CompletedTask;
    }
}
