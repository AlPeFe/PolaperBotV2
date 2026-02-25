using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Services;
using PolaperBot.Core.AI.Tools;

namespace PolaperBot.Core.AI.Extensions;

public static class ToolsExtensions
{
    public static IServiceCollection AddAgentTools(this IServiceCollection services, GoogleOptions? googleOptions = null)
    {
        googleOptions ??= new GoogleOptions();

        services.AddSingleton(googleOptions);
        services.AddSingleton<IGoogleServicesFactory, GoogleServicesFactory>();
        services.AddSingleton<GoogleServicesFactory>();

        services.AddSingleton<IMemoryService>(sp =>
        {
            var options = sp.GetRequiredService<AgentOptions>();
            return new MemoryService(options.MemoryPath);
        });

        services.AddSingleton<MemoryTool>();
        services.AddSingleton<GmailTool>();
        services.AddSingleton<GoogleCalendarTool>();

        return services;
    }

    public static AITool[] BuildAgentTools(this IServiceProvider serviceProvider)
    {
        var tools = new List<AITool>();

        var memoryTool = serviceProvider.GetRequiredService<MemoryTool>();
        tools.AddRange(memoryTool.AsAITools());

        var gmailTool = serviceProvider.GetRequiredService<GmailTool>();
        tools.AddRange(gmailTool.AsAITools());

        var calendarTool = serviceProvider.GetRequiredService<GoogleCalendarTool>();
        tools.AddRange(calendarTool.AsAITools());

        return tools.ToArray();
    }
}
