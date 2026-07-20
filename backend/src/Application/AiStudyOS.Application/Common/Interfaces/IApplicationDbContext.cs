using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Identity;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Goal> Goals { get; }
    DbSet<DailyTask> DailyTasks { get; }
    DbSet<PlannerRecommendation> PlannerRecommendations { get; }
    DbSet<AiTelemetryEvent> AiTelemetryEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
