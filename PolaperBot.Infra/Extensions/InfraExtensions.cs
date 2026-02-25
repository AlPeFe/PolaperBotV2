using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Sessions;
using PolaperBot.Infra.Sessions;

namespace PolaperBot.Infra;

public static class InfraExtensions
{
    public static IServiceCollection AddSqliteSessionStore(this IServiceCollection services, string dbPath)
    {
        services.AddSingleton<ISessionStore>(sp =>
        {
            var agent = sp.GetRequiredService<Microsoft.Agents.AI.AIAgent>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqliteSessionStore>>();
            return new SqliteSessionStore(agent, dbPath, logger);
        });

        return services;
    }
}
