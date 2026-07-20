using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Identity;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<DailyTask> DailyTasks => Set<DailyTask>();
    public DbSet<PlannerRecommendation> PlannerRecommendations => Set<PlannerRecommendation>();
    public DbSet<AiTelemetryEvent> AiTelemetryEvents => Set<AiTelemetryEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
