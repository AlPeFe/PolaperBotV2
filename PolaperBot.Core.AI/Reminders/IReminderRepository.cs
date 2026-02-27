namespace PolaperBot.Core.AI.Reminders;

public interface IReminderRepository
{
    Task<int> CreateAsync(Reminder reminder, CancellationToken cancellationToken = default);
    Task<Reminder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reminder>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reminder>> GetUpcomingAsync(TimeSpan advanceNotice, CancellationToken cancellationToken = default);
    Task MarkAsNotifiedAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
