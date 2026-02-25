using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Tools;

namespace PolaperBot.Core.AI.Extensions;

public static class ToolsExtensions
{
    public static IServiceCollection AddAgentTools(this IServiceCollection services)
    {
        services.AddSingleton<IMemoryService>(sp =>
        {
            var options = sp.GetRequiredService<AgentOptions>();
            return new MemoryService(options.MemoryPath);
        });

        services.AddSingleton<MemoryTool>();

        return services;
    }

    public static AITool[] BuildAgentTools(this IServiceProvider serviceProvider)
    {
        var tools = new List<AITool>();
        
        var memoryTool = serviceProvider.GetRequiredService<MemoryTool>();
        tools.AddRange(memoryTool.AsAITools());

        return tools.ToArray();
    }
}
