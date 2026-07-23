using AiStudyOS.Application.AI.Memory;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Mentor;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Infrastructure.AI.Memory;

public class PostgresMemoryStore(IApplicationDbContext db) : IMemoryStore
{
    public async Task<IReadOnlyList<MemoryRecordDto>> QueryAsync(MemoryQuery query, CancellationToken ct)
    {
        var records = db.MemoryRecords.Where(m => m.UserId == query.UserId);

        if (query.Type is not null)
            records = records.Where(m => m.Type == query.Type);

        if (!string.IsNullOrWhiteSpace(query.Topic))
            records = records.Where(m => m.Topic == query.Topic);

        var ordered = records.OrderByDescending(m => m.Salience).ThenByDescending(m => m.CreatedAtUtc);
        var limited = query.Take is { } take ? ordered.Take(take) : ordered;

        var results = await limited.ToListAsync(ct);

        return results
            .Select(m => new MemoryRecordDto(m.UserId, m.Type, m.Topic, m.Content, m.Salience, m.SourceType, m.CreatedAtUtc, m.SourceId))
            .ToList();
    }

    public async Task WriteAsync(MemoryRecordDto record, CancellationToken ct)
    {
        var entity = MemoryRecord.Create(record.UserId, record.Type, record.Topic, record.Content, record.Salience, record.SourceType, record.SourceId, record.CreatedAtUtc);
        db.MemoryRecords.Add(entity);
        await db.SaveChangesAsync(ct);
    }
}
