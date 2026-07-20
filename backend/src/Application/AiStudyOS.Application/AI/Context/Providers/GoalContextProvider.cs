using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Goals;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.AI.Context.Providers;

public class GoalContextProvider(IApplicationDbContext db) : IContextProvider
{
    public string SectionName => "Active Goals";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var goals = await db.Goals
            .Where(g => g.UserId == request.UserId && g.Status == GoalStatus.Active)
            .OrderBy(g => g.TargetDate ?? DateOnly.MaxValue)
            .ToListAsync(ct);

        if (goals.Count == 0)
        {
            const string empty = "The student has no active goals yet.";
            return new ContextFragment(SectionName, empty, EstimatedTokens: empty.Length / 4, Priority: 100);
        }

        var sb = new StringBuilder();
        foreach (var goal in goals)
        {
            sb.AppendLine($"- id: {goal.Id}");
            sb.AppendLine($"  title: {goal.Title}");
            if (!string.IsNullOrWhiteSpace(goal.Description))
                sb.AppendLine($"  description: {goal.Description}");
            sb.AppendLine($"  category: {goal.Category}");
            sb.AppendLine($"  priority: {goal.Priority}");
            sb.AppendLine($"  targetDate: {(goal.TargetDate.HasValue ? goal.TargetDate.Value.ToString("yyyy-MM-dd") : "none")}");
        }

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 100);
    }
}
