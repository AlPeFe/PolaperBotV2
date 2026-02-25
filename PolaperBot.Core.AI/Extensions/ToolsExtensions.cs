using Google.Apis.Calendar.v3;
using Google.Apis.Gmail.v1;
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

#pragma warning disable CS8634
        services.AddSingleton<GmailService?>(sp =>
        {
            var factory = sp.GetRequiredService<GoogleServicesFactory>();
            return factory.CreateGmailServiceAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<CalendarService?>(sp =>
        {
            var factory = sp.GetRequiredService<GoogleServicesFactory>();
            return factory.CreateCalendarServiceAsync().GetAwaiter().GetResult();
        });
#pragma warning restore CS8634

        services.AddSingleton<IMemoryService>(sp =>
        {
            var options = sp.GetRequiredService<AgentOptions>();
            return new MemoryService(options.MemoryPath);
        });

        services.AddSingleton<MemoryTool>();
        services.AddSingleton<GmailTool>();
        services.AddSingleton<GoogleCalendarTool>();
        services.AddSingleton<BashTool>();
        services.AddSingleton<FileSystemTool>();

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

        var bashTool = serviceProvider.GetRequiredService<BashTool>();
        tools.AddRange(bashTool.AsAITools());

        var fileSystemTool = serviceProvider.GetRequiredService<FileSystemTool>();
        tools.AddRange(fileSystemTool.AsAITools());

        return tools.ToArray();
    }
}
