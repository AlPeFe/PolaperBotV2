using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.HeartbeatTriggers;
using PolaperBot.Core.AI.Reminders;
using PolaperBot.Core.AI.Services;
using Telegram.Bot;

namespace PolaperBot.Core.AI.Extensions;

public static class TelegramExtensions
{
    public static IServiceCollection AddTelegramServices(
        this IServiceCollection services,
        TelegramOptions options)
    {
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(options.BotToken));
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<IHeartbeatTrigger, RemindersHbsTrigger>();

        return services;
    }
}
