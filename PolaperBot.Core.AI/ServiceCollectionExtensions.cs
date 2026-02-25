using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Extensions;

namespace PolaperBot.Core.AI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreAiAgent(this IServiceCollection services, Action<OllamaOptions>? configureOllama = null)
    {
        services.AddAgentInstructions();
        services.AddAgentTools();
        services.AddOllamaAgent(configureOllama);

        return services;
    }
}
