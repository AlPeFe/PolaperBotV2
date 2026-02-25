using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Configuration;

namespace PolaperBot.Core.AI.Extensions;

public static class InstructionsExtensions
{
    public static IServiceCollection AddAgentInstructions(this IServiceCollection services)
    {
        services.AddSingleton(new AgentOptions
        {
            Name = "HanniAssistant",
            Description = "Asistente conversacional natural",
            MemoryPath = "./MEMORY.MD"
        });

        return services;
    }
}
