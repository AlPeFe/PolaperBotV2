using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Configuration;

namespace PolaperBot.Core.AI.Services;

public interface IHeartbeatTrigger
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public class HeartbeatService : BackgroundService
{
    private readonly HeartbeatOptions _options;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly IEnumerable<IHeartbeatTrigger> _triggers;

    public HeartbeatService(
        IConfiguration configuration,
        ILogger<HeartbeatService> logger,
        IEnumerable<IHeartbeatTrigger> triggers)
    {
        _logger = logger;
        _triggers = triggers;

        _options = new HeartbeatOptions();
        configuration.GetSection("Heartbeat").Bind(_options);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Heartbeat service disabled");
            return;
        }

        _logger.LogInformation(
            "Heartbeat service started - Interval: {Interval}min, Triggers: {Triggers}",
            _options.IntervalMinutes,
            string.Join(", ", _options.EnabledTriggers));

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ExecuteTriggersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing heartbeat triggers");
            }
        }
    }

    private async Task ExecuteTriggersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing heartbeat triggers at {Time}", DateTimeOffset.Now);

        foreach (var triggerName in _options.EnabledTriggers)
        {
            var trigger = _triggers.FirstOrDefault(t => 
                t.Name.Equals(triggerName, StringComparison.OrdinalIgnoreCase));

            if (trigger == null)
            {
                _logger.LogWarning("Trigger not found: {TriggerName}", triggerName);
                continue;
            }

            try
            {
                _logger.LogInformation("Executing trigger: {TriggerName}", triggerName);
                await trigger.ExecuteAsync(cancellationToken);
                _logger.LogInformation("Trigger completed: {TriggerName}", triggerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing trigger: {TriggerName}", triggerName);
            }
        }
    }
}
