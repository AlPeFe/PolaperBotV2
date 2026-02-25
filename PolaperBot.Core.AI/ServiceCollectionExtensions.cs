using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Extensions;

namespace PolaperBot.Core.AI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreAiAgent(
        this IServiceCollection services,
        Action<OllamaOptions>? configureOllama = null,
        Action<GoogleOptions>? configureGoogle = null)
    {
        var googleOptions = new GoogleOptions();
        configureGoogle?.Invoke(googleOptions);

        services.AddAgentInstructions();
        services.AddAgentTools(googleOptions);
        services.AddOllamaAgent(configureOllama);

        return services;
    }
}
