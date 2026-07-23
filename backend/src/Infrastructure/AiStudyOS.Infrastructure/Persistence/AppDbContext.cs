using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Domain.Analytics;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Identity;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Domain.Quiz;
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
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<MemoryRecord> MemoryRecords => Set<MemoryRecord>();
    public DbSet<Domain.Quiz.Quiz> Quizzes => Set<Domain.Quiz.Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();
    public DbSet<TopicMastery> TopicMasteries => Set<TopicMastery>();
    public DbSet<TopicMasteryHistory> TopicMasteryHistories => Set<TopicMasteryHistory>();
    public DbSet<AnalyticsInsight> AnalyticsInsights => Set<AnalyticsInsight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
