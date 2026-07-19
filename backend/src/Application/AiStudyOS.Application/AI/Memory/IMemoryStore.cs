namespace AiStudyOS.Application.AI.Memory;

public enum MemoryType { LongTerm, UserProfile, Learning }

public record MemoryQuery(Guid UserId, MemoryType? Type = null, string? Topic = null, int? Take = null);

public record MemoryRecordDto(Guid UserId, MemoryType Type, string? Topic, string Content, double Salience, string SourceType, DateTime CreatedAtUtc);

public interface IMemoryStore
{
    Task<IReadOnlyList<MemoryRecordDto>> QueryAsync(MemoryQuery query, CancellationToken ct);
    Task WriteAsync(MemoryRecordDto record, CancellationToken ct);
}
