using AiStudyOS.Domain.Mentor;

namespace AiStudyOS.Application.AI.Memory;

public record MemoryQuery(Guid UserId, MemoryType? Type = null, string? Topic = null, int? Take = null);

public record MemoryRecordDto(Guid UserId, MemoryType Type, string? Topic, string Content, double Salience, string SourceType, DateTime CreatedAtUtc, Guid? SourceId = null);

public interface IMemoryStore
{
    Task<IReadOnlyList<MemoryRecordDto>> QueryAsync(MemoryQuery query, CancellationToken ct);
    Task WriteAsync(MemoryRecordDto record, CancellationToken ct);
}
