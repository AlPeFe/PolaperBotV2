using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Reminders;
using Telegram.Bot;

namespace PolaperBot.Core.AI.Services;

public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(ITelegramBotClient botClient, ILogger<TelegramService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<bool> SendMessageAsync(long chatId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
            _logger.LogInformation("Message sent to chat {ChatId}", chatId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
            return false;
        }
    }
}
