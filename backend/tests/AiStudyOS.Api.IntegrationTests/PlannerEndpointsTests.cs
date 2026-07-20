using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiStudyOS.Api.Endpoints;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Planner.Commands.RescheduleOverdueTasks;
using AiStudyOS.Application.Planner.Dtos;
using AiStudyOS.Domain.Goals;
using AiStudyOS.Domain.Planner;
using AiStudyOS.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AiStudyOS.Api.IntegrationTests;

/// <summary>
/// Exercises the M6 planner-intelligence read logic (focus score, completion %, streak, overdue
/// detection, week workload balance, recommendation history, smart reschedule) against a real
/// Postgres instance — these are LINQ queries that need to actually translate to SQL, not just
/// compile, and several (the streak walk in particular) are worth verifying against real data.
/// </summary>
public class PlannerEndpointsTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// Registers a user AND seeds an already-active recommendation for today, so
    /// GetTodayQueryHandler's cache check finds it and never falls through to real AI generation.
    /// These tests exercise pure read/query logic (focus score, streak, overdue, workload) — they
    /// must be deterministic regardless of whether a real Ollama happens to be reachable, and must
    /// never have their seeded task counts polluted by an unrelated auto-generated recommendation.
    /// </summary>
    private async Task<(HttpClient Client, Guid UserId)> RegisterOnlyAsync()
    {
        var email = $"planner-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "correct-horse-battery", "Planner Test"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return (client, body.User.Id);
    }

    private async Task<(HttpClient Client, Guid UserId)> AuthenticatedClientAsync()
    {
        var (client, userId) = await RegisterOnlyAsync();
        await SeedActiveRecommendationAsync(userId);
        return (client, userId);
    }

    private async Task SeedActiveRecommendationAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var recommendation = PlannerRecommendation.Create(
            userId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Test situation analysis.",
            "Test goal alignment.",
            "Test evidence.",
            "Test recommendation.",
            "Test immediate next action.",
            recommendedTaskId: null,
            modelUsed: "test-model",
            provider: "test",
            promptVersion: "v1",
            confidenceScore: 0.9,
            recommendationReason: "Seeded directly for test isolation from real AI generation.",
            generationTimeMs: 1,
            rawResponseJson: "{}",
            nowUtc: DateTime.UtcNow);

        db.PlannerRecommendations.Add(recommendation);
        await db.SaveChangesAsync();
    }

    private static async Task<Guid> CreateGoalAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/goals", new CreateGoalRequest("Test goal", null, GoalCategory.Skill, GoalPriority.Medium, null));
        response.EnsureSuccessStatusCode();
        var goal = await response.Content.ReadFromJsonAsync<GoalResponse>();
        return goal!.Id;
    }

    private record GoalResponse(Guid Id);

    /// <summary>Directly inserts a DailyTask, bypassing PlannerTool — legitimate for seeding
    /// historical data (past dates, specific statuses) that the API itself has no route to create.</summary>
    private async Task<Guid> SeedTaskAsync(Guid userId, DateOnly date, DailyTaskStatus status, int estimatedMinutes = 30, Guid? goalId = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var task = DailyTask.Create(userId, goalId, "Seeded task", null, date, estimatedMinutes, TaskSource.Manual, DateTime.UtcNow);
        if (status == DailyTaskStatus.Completed) task.Complete(DateTime.UtcNow);
        else if (status == DailyTaskStatus.Skipped) task.Skip(DateTime.UtcNow);

        db.DailyTasks.Add(task);
        await db.SaveChangesAsync();
        return task.Id;
    }

    [Fact]
    public async Task GetToday_NoTasks_ReturnsZeroedStats()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/planner/today");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var plan = await response.Content.ReadFromJsonAsync<TodayPlanDto>();
        plan!.DailyCompletionPercent.Should().Be(0);
        plan.DailyFocusScore.Should().Be(0);
        plan.StudyStreak.Should().Be(0);
        plan.OverdueTasks.Should().BeEmpty();
        plan.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetToday_DetectsOverdueTasks()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        await SeedTaskAsync(userId, yesterday, DailyTaskStatus.Pending);

        var response = await client.GetAsync("/api/v1/planner/today");
        var plan = await response.Content.ReadFromJsonAsync<TodayPlanDto>();

        plan!.OverdueTasks.Should().ContainSingle();
        plan.OverdueTasks[0].IsOverdue.Should().BeTrue();
        plan.OverdueTasks[0].Date.Should().Be(yesterday);
    }

    [Fact]
    public async Task GetToday_ComputesCompletionPercentAndFocusScore_WeightedByMinutes()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 90 completed minutes, 30 pending minutes: 1 of 2 tasks done (50% completion),
        // 90 of 120 total minutes done (75% focus score).
        await SeedTaskAsync(userId, today, DailyTaskStatus.Completed, estimatedMinutes: 90);
        await SeedTaskAsync(userId, today, DailyTaskStatus.Pending, estimatedMinutes: 30);

        var response = await client.GetAsync("/api/v1/planner/today");
        var plan = await response.Content.ReadFromJsonAsync<TodayPlanDto>();

        plan!.DailyCompletionPercent.Should().Be(50.0);
        plan.DailyFocusScore.Should().Be(75);
    }

    [Fact]
    public async Task GetToday_ComputesStudyStreak_AcrossConsecutiveDays()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTaskAsync(userId, today, DailyTaskStatus.Completed);
        await SeedTaskAsync(userId, today.AddDays(-1), DailyTaskStatus.Completed);
        await SeedTaskAsync(userId, today.AddDays(-2), DailyTaskStatus.Completed);
        // Gap at day -3 — streak must stop there, not continue into day -4.
        await SeedTaskAsync(userId, today.AddDays(-4), DailyTaskStatus.Completed);

        var response = await client.GetAsync("/api/v1/planner/today");
        var plan = await response.Content.ReadFromJsonAsync<TodayPlanDto>();

        plan!.StudyStreak.Should().Be(3);
    }

    [Fact]
    public async Task GetToday_StreakNotBrokenByUnfinishedToday_IfYesterdayCompleted()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Nothing completed today yet (day still in progress) but yesterday was completed.
        await SeedTaskAsync(userId, today.AddDays(-1), DailyTaskStatus.Completed);
        await SeedTaskAsync(userId, today, DailyTaskStatus.Pending);

        var response = await client.GetAsync("/api/v1/planner/today");
        var plan = await response.Content.ReadFromJsonAsync<TodayPlanDto>();

        plan!.StudyStreak.Should().Be(1, "yesterday's completion should still count even though today isn't done yet");
    }

    [Fact]
    public async Task RescheduleOverdueTasks_MovesAllOverdueTasksToToday()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTaskAsync(userId, today.AddDays(-3), DailyTaskStatus.Pending);
        await SeedTaskAsync(userId, today.AddDays(-1), DailyTaskStatus.Pending);
        await SeedTaskAsync(userId, today, DailyTaskStatus.Pending); // not overdue — must be untouched

        var response = await client.PostAsync("/api/v1/planner/tasks/reschedule-overdue", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RescheduleOverdueTasksResultDto>();
        result!.RescheduledCount.Should().Be(2);

        // Each reschedule invalidates today's recommendation (existing M5 behavior) — re-seed an
        // active one so this test's own GET /today doesn't fall through to real AI generation.
        await SeedActiveRecommendationAsync(userId);

        var todayResponse = await client.GetAsync("/api/v1/planner/today");
        var plan = await todayResponse.Content.ReadFromJsonAsync<TodayPlanDto>();
        plan!.OverdueTasks.Should().BeEmpty();
        plan.Tasks.Should().HaveCount(3, "the 2 rescheduled tasks plus the 1 already-today task should all now be dated today");
    }

    [Fact]
    public async Task GetWeek_ComputesWorkloadBalanceAndCompletionPercent()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTaskAsync(userId, today, DailyTaskStatus.Completed, estimatedMinutes: 300); // over the 240min default threshold
        await SeedTaskAsync(userId, today.AddDays(1), DailyTaskStatus.Pending, estimatedMinutes: 60);

        var response = await client.GetAsync("/api/v1/planner/week");
        var week = await response.Content.ReadFromJsonAsync<WeekDto>();

        var todayEntry = week!.Days.Single(d => d.Date == today);
        todayEntry.TotalEstimatedMinutes.Should().Be(300);
        todayEntry.IsOverloaded.Should().BeTrue();

        var tomorrowEntry = week.Days.Single(d => d.Date == today.AddDays(1));
        tomorrowEntry.TotalEstimatedMinutes.Should().Be(60);
        tomorrowEntry.IsOverloaded.Should().BeFalse();

        week.WeeklyCompletionPercent.Should().Be(50.0);
    }

    [Fact]
    public async Task GetRecommendationHistory_ReturnsEmptyForNewUser()
    {
        var (client, _) = await RegisterOnlyAsync();

        var response = await client.GetAsync("/api/v1/planner/recommendations/history");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await response.Content.ReadFromJsonAsync<List<PlannerRecommendationDto>>();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecommendationHistory_ReturnsSeededRecommendation_NewestFirst()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/planner/recommendations/history");
        var history = await response.Content.ReadFromJsonAsync<List<PlannerRecommendationDto>>();

        history.Should().ContainSingle(r => r.RecommendationReason == "Seeded directly for test isolation from real AI generation.");
    }

    [Fact]
    public async Task GetToday_TaskLinkedToGoal_IncludesGoalTitle()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var goalId = await CreateGoalAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Creating a goal invalidates today's recommendation (existing M5 behavior) — re-seed an
        // active one so this test's GET /today doesn't fall through to real AI generation.
        await SeedActiveRecommendationAsync(userId);
        await SeedTaskAsync(userId, today, DailyTaskStatus.Pending, goalId: goalId);

        var response = await client.GetAsync("/api/v1/planner/today");
        var plan = await response.Content.ReadFromJsonAsync<TodayPlanDto>();

        plan!.Tasks.Should().ContainSingle(t => t.GoalId == goalId && t.GoalTitle == "Test goal");
    }
}
