using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Extensions;

namespace PolaperBot.Core.AI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreAiAgent(
        this IServiceCollection services,
        Action<OllamaOptions>? configureOllama = null,
        Action<GoogleOptions>? configureGoogle = null,
        Action<TelegramOptions>? configureTelegram = null)
    {
        var googleOptions = new GoogleOptions();
        configureGoogle?.Invoke(googleOptions);

        var telegramOptions = new TelegramOptions();
        configureTelegram?.Invoke(telegramOptions);

        services.AddAgentInstructions();
        services.AddAgentTools(googleOptions);
        services.AddOllamaAgent(configureOllama);

        if (!string.IsNullOrEmpty(telegramOptions.BotToken))
        {
            services.AddTelegramServices(telegramOptions);
        }

        return services;
    }
}
