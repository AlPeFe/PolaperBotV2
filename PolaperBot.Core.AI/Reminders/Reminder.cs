namespace PolaperBot.Core.AI.Reminders;

public class Reminder
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public DateTime CreatedAt { get; set; }
    public long TelegramChatId { get; set; }
    public bool IsNotified { get; set; }
    public DateTime? NotifiedAt { get; set; }
}
