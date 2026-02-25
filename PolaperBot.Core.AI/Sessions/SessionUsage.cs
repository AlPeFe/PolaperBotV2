namespace PolaperBot.Core.AI.Sessions;

public class SessionUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens => InputTokens + OutputTokens;
    public string? Model { get; set; }
    public double ProcessingTimeMs { get; set; }
    public int ToolCalls { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
