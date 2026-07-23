using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.AI.Context.Providers;

public class TopicMasteryContextProvider(IApplicationDbContext db) : IContextProvider
{
    public string SectionName => "Topic Mastery";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var mastery = await db.TopicMasteries
            .Where(m => m.UserId == request.UserId)
            .OrderBy(m => m.MasteryScore)
            .ToListAsync(ct);

        if (mastery.Count == 0)
        {
            const string empty = "No mastery history yet — this is the student's first quiz.";
            return new ContextFragment(SectionName, empty, EstimatedTokens: empty.Length / 4, Priority: 70);
        }

        var sb = new StringBuilder();
        foreach (var m in mastery)
            sb.AppendLine($"- topic: {m.Topic}, masteryScore: {m.MasteryScore:F2} (0=weak, 1=strong), attempts: {m.AttemptsCount}");

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 70);
    }
}
