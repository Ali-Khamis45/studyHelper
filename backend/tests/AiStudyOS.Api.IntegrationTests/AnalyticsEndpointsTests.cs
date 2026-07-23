using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AiStudyOS.Api.Endpoints;
using AiStudyOS.Application.Analytics.Dtos;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Domain.Quiz;
using AiStudyOS.Domain.Telemetry;
using AiStudyOS.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiStudyOS.Api.IntegrationTests;

/// <summary>
/// Exercises the M9 Analytics module: every stat is asserted against deterministic, directly-seeded
/// rows (not real AI output) so the numbers can be checked exactly — except Insights generation
/// and export, which go through the real pipeline once each. Mirrors QuizEndpointsTests/
/// MentorEndpointsTests' seed-directly-except-where-AI-output-is-the-point approach.
/// </summary>
public class AnalyticsEndpointsTests(GenerousRateLimitAuthApiFactory factory) : IClassFixture<GenerousRateLimitAuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(HttpClient Client, Guid UserId)> AuthenticatedClientAsync()
    {
        var email = $"analytics-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "correct-horse-battery", "Analytics Test"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return (client, body.User.Id);
    }

    private static AppDbContext Db(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // ---------------------------------------------------------------------------------------
    // Study time / task stats / streak
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Weekly_ComputesStudyTimeAndTaskStatsFromRealTasks()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            var completed = DailyTask.Create(userId, null, "Completed task", null, today, 45, TaskSource.Manual, DateTime.UtcNow);
            completed.Complete(DateTime.UtcNow);
            var skipped = DailyTask.Create(userId, null, "Skipped task", null, today, 20, TaskSource.Manual, DateTime.UtcNow);
            skipped.Skip(DateTime.UtcNow);
            var rescheduled = DailyTask.Create(userId, null, "Rescheduled task", null, today.AddDays(-1), 15, TaskSource.Manual, DateTime.UtcNow);
            rescheduled.Reschedule(today, DateTime.UtcNow);

            db.DailyTasks.AddRange(completed, skipped, rescheduled);
            await db.SaveChangesAsync();
        }

        var weekly = await (await client.GetAsync("/api/v1/analytics/weekly")).Content.ReadFromJsonAsync<PeriodAnalyticsDto>();

        weekly!.StudyTime.DailyMinutes.Should().Be(45);
        weekly.Tasks.Completed.Should().Be(1);
        weekly.Tasks.Skipped.Should().Be(1);
        weekly.Tasks.Rescheduled.Should().Be(1);
        weekly.Tasks.Total.Should().Be(3);
        weekly.DailyActivity.Should().HaveCount(7);
    }

    [Fact]
    public async Task Streak_ComputesCurrentAndLongestFromRealCompletions()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);

            // A 4-day run further in the past (the "longest"), then a gap, then a shorter 2-day run
            // ending today (the "current") — deliberately different lengths so the test actually
            // discriminates between the two computations rather than them coincidentally matching.
            foreach (var offset in new[] { -12, -11, -10, -9, -1, 0 })
            {
                var date = today.AddDays(offset);
                var task = DailyTask.Create(userId, null, $"Task {offset}", null, date, 10, TaskSource.Manual, DateTime.UtcNow);
                task.Complete(DateTime.UtcNow);
                db.DailyTasks.Add(task);
            }

            await db.SaveChangesAsync();
        }

        var streak = await (await client.GetAsync("/api/v1/analytics/streak")).Content.ReadFromJsonAsync<StreakAnalyticsDto>();

        streak!.CurrentStreak.Should().Be(2); // offsets -1, 0
        streak.LongestStreak.Should().Be(4); // offsets -12, -11, -10, -9
        streak.CompletionHeatmap.Should().NotBeEmpty();
    }

    // ---------------------------------------------------------------------------------------
    // Goals
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Goals_ComputesCompletionPercentFromRealGoals()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            var active = Goal.Create(userId, "Active goal", null, GoalCategory.Academic, GoalPriority.Medium, null, DateTime.UtcNow);
            var completed = Goal.Create(userId, "Completed goal", null, GoalCategory.Academic, GoalPriority.Medium, null, DateTime.UtcNow);
            completed.SetStatus(GoalStatus.Completed, DateTime.UtcNow);
            db.Goals.AddRange(active, completed);
            await db.SaveChangesAsync();
        }

        var goals = await (await client.GetAsync("/api/v1/analytics/goals")).Content.ReadFromJsonAsync<GoalAnalyticsDto>();

        goals!.TotalGoals.Should().Be(2);
        goals.CompletedGoals.Should().Be(1);
        goals.CompletionPercent.Should().Be(50);
    }

    // ---------------------------------------------------------------------------------------
    // Quizzes / mastery
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Quizzes_ComputesScoreStatsFromRealAttempts()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            var quiz = Domain.Quiz.Quiz.Create(userId, null, "Quiz", "Topic", Difficulty.Easy, QuizType.Standard, 2, "test", "v1", "corr", DateTime.UtcNow);
            db.Quizzes.Add(quiz);

            // Two questions per attempt so 100/50/0 are all reachable exactly (correctCount/totalCount).
            foreach (var correctCount in new[] { 2, 1, 0 })
            {
                var attempt = QuizAttempt.Create(quiz.Id, userId, DateTime.UtcNow);
                attempt.Complete(correctCount, totalCount: 2, DateTime.UtcNow);
                db.QuizAttempts.Add(attempt);
            }

            await db.SaveChangesAsync();
        }

        var quizzes = await (await client.GetAsync("/api/v1/analytics/quizzes")).Content.ReadFromJsonAsync<QuizAnalyticsDto>();

        quizzes!.AttemptCount.Should().Be(3);
        quizzes.AverageScore.Should().BeApproximately(50, 0.1);
        quizzes.HighestScore.Should().Be(100);
        quizzes.LowestScore.Should().Be(0);
        quizzes.ScoreDistribution.Sum(d => d.Count).Should().Be(3);
    }

    [Fact]
    public async Task Mastery_SeparatesWeakAndStrongTopics()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            db.TopicMasteries.Add(TopicMastery.Create(userId, "Weak Topic", 0.3, DateTime.UtcNow));
            db.TopicMasteries.Add(TopicMastery.Create(userId, "Strong Topic", 0.9, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var mastery = await (await client.GetAsync("/api/v1/analytics/mastery")).Content.ReadFromJsonAsync<MasteryAnalyticsDto>();

        mastery!.WeakTopics.Should().ContainSingle(t => t.Topic == "Weak Topic");
        mastery.StrongTopics.Should().ContainSingle(t => t.Topic == "Strong Topic");
        mastery.Radar.Should().NotBeEmpty();
    }

    // ---------------------------------------------------------------------------------------
    // Planner effectiveness / mentor / AI usage
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Planner_ComputesAcceptanceRateFromRealRecommendationsAndTaskCompletion()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);

            var acceptedTask = DailyTask.Create(userId, null, "Accepted", null, today, 30, TaskSource.AiGenerated, DateTime.UtcNow);
            acceptedTask.Complete(DateTime.UtcNow);
            var ignoredTask = DailyTask.Create(userId, null, "Ignored", null, today, 30, TaskSource.AiGenerated, DateTime.UtcNow);
            db.DailyTasks.AddRange(acceptedTask, ignoredTask);
            await db.SaveChangesAsync();

            var acceptedRec = PlannerRecommendation.Create(userId, today, "s", "g", "e", "r", "i", acceptedTask.Id, "test-model", "test", "v1", 0.9, "reason", 120, "{}", DateTime.UtcNow);
            var ignoredRec = PlannerRecommendation.Create(userId, today.AddDays(-1), "s", "g", "e", "r", "i", ignoredTask.Id, "test-model", "test", "v1", 0.7, "reason", 80, "{}", DateTime.UtcNow.AddDays(-1));
            db.PlannerRecommendations.AddRange(acceptedRec, ignoredRec);
            await db.SaveChangesAsync();
        }

        var planner = await (await client.GetAsync("/api/v1/analytics/planner")).Content.ReadFromJsonAsync<PlannerAnalyticsDto>();

        planner!.RecommendationCount.Should().Be(2);
        planner.AcceptanceRatePercent.Should().Be(50);
        planner.AverageConfidence.Should().BeApproximately(0.8, 0.01);
    }

    [Fact]
    public async Task Mentor_ComputesConversationAndMessageStatsFromRealConversations()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            var now = DateTime.UtcNow;
            var conversation = Conversation.Create(userId, "Chat", now);
            conversation.RecordExchange(promptTokens: 100, completionTokens: 200, now.AddMinutes(10));
            db.Conversations.Add(conversation);
            await db.SaveChangesAsync();
        }

        var mentor = await (await client.GetAsync("/api/v1/analytics/mentor")).Content.ReadFromJsonAsync<MentorAnalyticsDto>();

        mentor!.ConversationCount.Should().Be(1);
        mentor.MessageCount.Should().Be(2);
        mentor.AverageSessionLengthMinutes.Should().BeApproximately(10, 0.1);
    }

    [Fact]
    public async Task AiUsage_IsScopedPerUser_NotGlobal()
    {
        var (clientA, userIdA) = await AuthenticatedClientAsync();
        var (_, userIdB) = await AuthenticatedClientAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            db.AiTelemetryEvents.Add(AiTelemetryEvent.Create("corr-a", AgentType.Tutor, "ollama", "llama3.1", "v1", 100, 200, 0m, 500, 0, 0, 0, true, null, DateTime.UtcNow, false, false, "Closed", null, null, userIdA));
            db.AiTelemetryEvents.Add(AiTelemetryEvent.Create("corr-b", AgentType.Tutor, "ollama", "llama3.1", "v1", 999, 999, 0m, 9999, 0, 0, 0, false, "Failure", DateTime.UtcNow, false, false, "Closed", null, null, userIdB));
            await db.SaveChangesAsync();
        }

        var overviewA = await (await clientA.GetAsync("/api/v1/analytics")).Content.ReadFromJsonAsync<AnalyticsOverviewDto>();

        overviewA!.Ai.TotalRequests.Should().Be(1);
        overviewA.Ai.SuccessRatePercent.Should().Be(100);
        overviewA.Ai.TotalPromptTokens.Should().Be(100);
    }

    // ---------------------------------------------------------------------------------------
    // Overview / Dashboard / Insights / Export
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Overview_ReturnsAllSections()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/analytics");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var overview = await response.Content.ReadFromJsonAsync<AnalyticsOverviewDto>();
        overview.Should().NotBeNull();
        overview!.StudyTime.Should().NotBeNull();
        overview.Tasks.Should().NotBeNull();
        overview.Goals.Should().NotBeNull();
        overview.Streak.Should().NotBeNull();
        overview.Quizzes.Should().NotBeNull();
        overview.Mastery.Should().NotBeNull();
        overview.Mentor.Should().NotBeNull();
        overview.Ai.Should().NotBeNull();
        overview.Planner.Should().NotBeNull();
        overview.Timeline.Should().NotBeNull();
    }

    [Fact]
    public async Task Overview_CustomDateRange_IsRespected()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var from = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-14);
        var to = DateOnly.FromDateTime(DateTime.UtcNow);

        var overview = await (await client.GetAsync($"/api/v1/analytics?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}")).Content.ReadFromJsonAsync<AnalyticsOverviewDto>();

        overview!.From.Should().Be(from);
        overview.To.Should().Be(to);
    }

    [Fact]
    public async Task Dashboard_ReturnsLightweightBundle()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/analytics/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboard = await response.Content.ReadFromJsonAsync<DashboardAnalyticsDto>();
        dashboard.Should().NotBeNull();
        dashboard!.WeeklyActivity.Should().HaveCount(7);
    }

    [Fact]
    public async Task Insights_RealAi_GeneratesAndCaches()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        // Seed a little real data so the Insights prompt has something concrete to analyze.
        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            db.Goals.Add(Goal.Create(userId, "Learn calculus", null, GoalCategory.Academic, GoalPriority.High, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var first = await (await client.PostAsync("/api/v1/analytics/insights/regenerate", null)).Content.ReadFromJsonAsync<InsightsDto>();
        first.Should().NotBeNull();
        first!.WeeklySummary.Should().NotBeNullOrWhiteSpace();
        first.MonthlySummary.Should().NotBeNullOrWhiteSpace();
        first.RiskDetection.Should().NotBeNullOrWhiteSpace();

        // The Overview endpoint must reuse the cached report rather than generating a second one.
        var overview = await (await client.GetAsync("/api/v1/analytics")).Content.ReadFromJsonAsync<AnalyticsOverviewDto>();
        overview!.Insights.Should().NotBeNull();
        overview.Insights!.WeeklySummary.Should().Be(first.WeeklySummary);
        // BeCloseTo, not Be: a JSON round-trip loses sub-microsecond DateTime precision, which
        // isn't a real discrepancy — the point of this assertion is "same cached row," not
        // bit-for-bit timestamp equality.
        overview.Insights.GeneratedAtUtc.Should().BeCloseTo(first.GeneratedAtUtc, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task ExportPdf_ReturnsValidPdfBytes()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/analytics/export/pdf");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        // PDF files always start with the "%PDF" magic bytes.
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public async Task ExportCsv_ReturnsValidCsvText()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/analytics/export/csv");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("AI Study OS");
        text.Should().Contain("Study Time");
        text.Should().Contain("Tasks");
    }

    // ---------------------------------------------------------------------------------------
    // Authorization / isolation
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Analytics_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/analytics");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Goals_NeverLeaksAnotherUsersGoalsIntoTotals()
    {
        var (clientA, userIdA) = await AuthenticatedClientAsync();
        var (_, userIdB) = await AuthenticatedClientAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = Db(scope);
            db.Goals.Add(Goal.Create(userIdA, "A's goal", null, GoalCategory.Academic, GoalPriority.Medium, null, DateTime.UtcNow));
            db.Goals.Add(Goal.Create(userIdB, "B's goal 1", null, GoalCategory.Academic, GoalPriority.Medium, null, DateTime.UtcNow));
            db.Goals.Add(Goal.Create(userIdB, "B's goal 2", null, GoalCategory.Academic, GoalPriority.Medium, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        var goalsA = await (await clientA.GetAsync("/api/v1/analytics/goals")).Content.ReadFromJsonAsync<GoalAnalyticsDto>();

        goalsA!.TotalGoals.Should().Be(1);
        goalsA.Goals.Should().ContainSingle(g => g.Title == "A's goal");
    }
}
