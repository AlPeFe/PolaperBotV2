namespace PolaperBot.Core.AI.Configuration;

public class HeartbeatOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 5;
    public List<string> EnabledTriggers { get; set; } = new();
}
