using System.Text;
using AiStudyOS.Application.AI.Memory;
using AiStudyOS.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.AI.Context.Providers;

public class MemoryContextProvider(IMemoryStore memoryStore, IOptions<MentorOptions> options) : IContextProvider
{
    public string SectionName => "Long-Term Memory";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var records = await memoryStore.QueryAsync(new MemoryQuery(request.UserId, Take: options.Value.MemoryContextTake), ct);

        if (records.Count == 0)
        {
            const string empty = "No long-term memory recorded about this student yet.";
            return new ContextFragment(SectionName, empty, EstimatedTokens: empty.Length / 4, Priority: 40);
        }

        var sb = new StringBuilder();
        foreach (var record in records.OrderByDescending(r => r.Salience).ThenByDescending(r => r.CreatedAtUtc))
        {
            sb.AppendLine($"- [{record.Type}{(record.Topic is null ? "" : $"/{record.Topic}")}] {record.Content}");
        }

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 40);
    }
}
