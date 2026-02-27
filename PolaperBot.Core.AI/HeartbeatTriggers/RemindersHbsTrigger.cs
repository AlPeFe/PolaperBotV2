using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Reminders;
using PolaperBot.Core.AI.Services;

namespace PolaperBot.Core.AI.HeartbeatTriggers;

public class RemindersHbsTrigger : IHeartbeatTrigger
{
    private readonly IReminderRepository _reminderRepository;
    private readonly ITelegramService _telegramService;
    private readonly ILogger<RemindersHbsTrigger> _logger;
    private static readonly TimeSpan AdvanceNotice = TimeSpan.FromMinutes(5);

    public string Name => "RemindersHbs";

    public RemindersHbsTrigger(
        IReminderRepository reminderRepository,
        ITelegramService telegramService,
        ILogger<RemindersHbsTrigger> logger)
    {
        _reminderRepository = reminderRepository;
        _telegramService = telegramService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RemindersHbs trigger executed - checking upcoming reminders");

        var upcomingReminders = await _reminderRepository.GetUpcomingAsync(AdvanceNotice, cancellationToken);

        if (upcomingReminders.Count == 0)
        {
            _logger.LogDebug("No upcoming reminders to notify");
            return;
        }

        _logger.LogInformation("Found {Count} reminders to notify", upcomingReminders.Count);

        foreach (var reminder in upcomingReminders)
        {
            try
            {
                var timeUntil = reminder.ScheduledFor - DateTime.UtcNow;
                var timeDescription = timeUntil.TotalMinutes <= 1
                    ? "ahora mismo"
                    : $"en {Math.Ceiling(timeUntil.TotalMinutes)} minutos";

                var message = $"🔔 **Recordatorio** ({timeDescription})\n\n{reminder.Description}";

                var sent = await _telegramService.SendMessageAsync(
                    reminder.TelegramChatId,
                    message,
                    cancellationToken);

                if (sent)
                {
                    await _reminderRepository.MarkAsNotifiedAsync(reminder.Id, cancellationToken);
                    _logger.LogInformation(
                        "Notified reminder {Id} to chat {ChatId}",
                        reminder.Id, reminder.TelegramChatId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send notification for reminder {Id}, will retry next cycle",
                        reminder.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminder {Id}", reminder.Id);
            }
        }
    }
}
