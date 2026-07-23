using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Mentor;

public class Conversation : AggregateRoot
{
    public const string DefaultTitle = "New conversation";

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public bool IsPinned { get; private set; }
    public int MessageCount { get; private set; }
    public int TotalPromptTokens { get; private set; }
    public int TotalCompletionTokens { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? LastMessageAtUtc { get; private set; }

    private Conversation() { }

    public static Conversation Create(Guid userId, string? title, DateTime nowUtc) => new()
    {
        UserId = userId,
        Title = string.IsNullOrWhiteSpace(title) ? DefaultTitle : title.Trim(),
        IsPinned = false,
        MessageCount = 0,
        TotalPromptTokens = 0,
        TotalCompletionTokens = 0,
        CreatedAtUtc = nowUtc,
        UpdatedAtUtc = nowUtc,
        LastMessageAtUtc = null,
    };

    public void Rename(string title, DateTime nowUtc)
    {
        Title = title.Trim();
        UpdatedAtUtc = nowUtc;
    }

    public void SetPinned(bool isPinned, DateTime nowUtc)
    {
        IsPinned = isPinned;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Called once per completed user/assistant exchange (two persisted messages).</summary>
    public void RecordExchange(int promptTokens, int completionTokens, DateTime nowUtc)
    {
        MessageCount += 2;
        TotalPromptTokens += promptTokens;
        TotalCompletionTokens += completionTokens;
        LastMessageAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public bool HasDefaultTitle => Title == DefaultTitle;
}
