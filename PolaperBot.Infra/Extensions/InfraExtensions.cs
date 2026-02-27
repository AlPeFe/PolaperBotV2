using Microsoft.Extensions.DependencyInjection;
using PolaperBot.Core.AI.Reminders;
using PolaperBot.Core.AI.Sessions;
using PolaperBot.Infra.Reminders;

namespace PolaperBot.Infra;

public static class InfraExtensions
{
    public static IServiceCollection AddSqliteSessionStore(this IServiceCollection services, string dbPath)
    {
        services.AddSingleton<ISessionStore>(sp =>
        {
            var agent = sp.GetRequiredService<Microsoft.Agents.AI.AIAgent>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Sessions.SqliteSessionStore>>();
            return new Sessions.SqliteSessionStore(agent, dbPath, logger);
        });

        return services;
    }

    public static IServiceCollection AddSqliteReminderRepository(this IServiceCollection services, string dbPath)
    {
        services.AddSingleton<IReminderRepository>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqliteReminderRepository>>();
            return new SqliteReminderRepository(dbPath, logger);
        });

        return services;
    }
}
