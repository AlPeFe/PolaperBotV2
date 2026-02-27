namespace PolaperBot.Core.AI.Reminders;

public interface ITelegramService
{
    Task<bool> SendMessageAsync(long chatId, string message, CancellationToken cancellationToken = default);
}
