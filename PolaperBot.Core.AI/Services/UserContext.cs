namespace PolaperBot.Core.AI.Services;

public interface IUserContext
{
    long CurrentChatId { get; set; }
}

public class UserContext : IUserContext
{
    private static readonly AsyncLocal<long> _currentChatId = new();

    public long CurrentChatId
    {
        get => _currentChatId.Value;
        set => _currentChatId.Value = value;
    }
}
