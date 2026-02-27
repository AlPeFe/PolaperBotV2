using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.HeartbeatTriggers;
using PolaperBot.Core.AI.Services;

namespace PolaperBot.Core.AI.Extensions;

public static class HeartbeatExtensions
{
    public static IServiceCollection AddHeartbeatService(this IServiceCollection services)
    {
        services.AddSingleton<IHeartbeatTrigger, GmailHbsTrigger>();
        services.AddHostedService<HeartbeatService>();

        return services;
    }
}
