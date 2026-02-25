using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Services;

namespace PolaperBot.Core.AI.HeartbeatTriggers;

public class RemindersHbsTrigger : IHeartbeatTrigger
{
    private readonly ILogger<RemindersHbsTrigger> _logger;

    public string Name => "RemindersHbs";

    public RemindersHbsTrigger(ILogger<RemindersHbsTrigger> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RemindersHbs trigger executed - checking reminders");
        
        return Task.CompletedTask;
    }
}
