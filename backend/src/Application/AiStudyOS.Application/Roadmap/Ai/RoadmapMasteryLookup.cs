using AiStudyOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Roadmap.Ai;

/// <summary>One query, shared by GetRoadmapQueryHandler and GetRoadmapsQueryHandler, building the same lowercase-topic-name -> mastery-score map RoadmapProgressCalculator expects.</summary>
public static class RoadmapMasteryLookup
{
    public static async Task<Dictionary<string, double>> BuildAsync(IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var rows = await db.TopicMasteries.Where(m => m.UserId == userId).ToListAsync(ct);
        return rows
            .GroupBy(m => m.Topic.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.Max(m => m.MasteryScore));
    }
}
