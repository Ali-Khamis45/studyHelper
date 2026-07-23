using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Mentor;

/// <summary>Durable, cross-conversation fact about a student, written by agents whose AgentDefinition.MemoryAccess.CanWrite is true and read back through ContextBuilder on later requests.</summary>
public class MemoryRecord : AggregateRoot
{
    public Guid UserId { get; private set; }
    public MemoryType Type { get; private set; }
    public string? Topic { get; private set; }
    public string Content { get; private set; } = null!;
    public double Salience { get; private set; }
    public string SourceType { get; private set; } = null!;
    public Guid? SourceId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private MemoryRecord() { }

    public static MemoryRecord Create(
        Guid userId,
        MemoryType type,
        string? topic,
        string content,
        double salience,
        string sourceType,
        Guid? sourceId,
        DateTime nowUtc) => new()
    {
        UserId = userId,
        Type = type,
        Topic = topic,
        Content = content,
        Salience = Math.Clamp(salience, 0d, 1d),
        SourceType = sourceType,
        SourceId = sourceId,
        CreatedAtUtc = nowUtc,
    };
}
