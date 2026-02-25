using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Services;

namespace PolaperBot.Core.AI.Extensions;

public static class AgentBuilderExtensions
{
    private const int MaxMessageCount = 20;

    public static IServiceCollection AddOllamaAgent(this IServiceCollection services, Action<OllamaOptions>? configure = null)
    {
        var ollamaOptions = new OllamaOptions();
        configure?.Invoke(ollamaOptions);

        services.AddSingleton(ollamaOptions);

        services.AddSingleton<AIAgent>(sp =>
        {
            var agentOptions = sp.GetRequiredService<AgentOptions>();
            var tools = sp.BuildAgentTools();

            var chatClient = new OllamaApiClient(
                new Uri(ollamaOptions.Endpoint),
                ollamaOptions.Model);

#pragma warning disable MEAI001
            var historyProvider = new InMemoryChatHistoryProvider(
                new InMemoryChatHistoryProviderOptions 
                { 
                    ChatReducer = new MessageCountingChatReducer(MaxMessageCount) 
                });

            var agentOptionsConfig = new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions 
                { 
                    Instructions = AgentInstructions.HanniAssistant,
                    Tools = tools
                },
                Name = agentOptions.Name,
                ChatHistoryProvider = historyProvider
            };
#pragma warning restore MEAI001

            return chatClient.AsAIAgent(agentOptionsConfig);
        });

        services.AddScoped<IAgentService, AgentService>();

        return services;
    }
}
