using AiStudyOS.Domain.Analytics;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Identity;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Domain.Quiz;
using AiStudyOS.Domain.Roadmap;
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
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }
    DbSet<MemoryRecord> MemoryRecords { get; }
    DbSet<Domain.Quiz.Quiz> Quizzes { get; }
    DbSet<QuizQuestion> QuizQuestions { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<QuizAnswer> QuizAnswers { get; }
    DbSet<TopicMastery> TopicMasteries { get; }
    DbSet<TopicMasteryHistory> TopicMasteryHistories { get; }
    DbSet<AnalyticsInsight> AnalyticsInsights { get; }
    DbSet<LearningRoadmap> LearningRoadmaps { get; }
    DbSet<RoadmapTopic> RoadmapTopics { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
