using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.AI.Context.Providers;

public class TaskContextProvider(IApplicationDbContext db, IDateTimeProvider dateTimeProvider, IOptions<PlannerOptions> plannerOptions) : IContextProvider
{
    public string SectionName => "Recent Task History";

    public async Task<ContextFragment> BuildAsync(ContextRequest request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var since = today.AddDays(-plannerOptions.Value.RecentHistoryDays);

        var tasks = await db.DailyTasks
            .Where(t => t.UserId == request.UserId && t.Date >= since && t.Date <= today)
            .OrderByDescending(t => t.Date)
            .ToListAsync(ct);

        if (tasks.Count == 0)
        {
            const string empty = "No recent task history — this would be the student's first planned day.";
            return new ContextFragment(SectionName, empty, EstimatedTokens: empty.Length / 4, Priority: 80);
        }

        var sb = new StringBuilder();
        foreach (var task in tasks)
        {
            sb.AppendLine($"- date: {task.Date:yyyy-MM-dd}, status: {task.Status}, goalId: {(task.GoalId?.ToString() ?? "none")}, title: {task.Title}");
        }

        var content = sb.ToString().TrimEnd();
        return new ContextFragment(SectionName, content, EstimatedTokens: content.Length / 4, Priority: 80);
    }
}
