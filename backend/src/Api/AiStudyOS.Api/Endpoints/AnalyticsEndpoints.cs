using AiStudyOS.Application.Analytics.Commands.RegenerateInsights;
using AiStudyOS.Application.Analytics.Queries.ExportAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetAnalyticsOverview;
using AiStudyOS.Application.Analytics.Queries.GetDashboardAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetGoalAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetMasteryAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetMentorAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetMonthlyAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetPlannerAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetQuizAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetStreakAnalytics;
using AiStudyOS.Application.Analytics.Queries.GetWeeklyAnalytics;
using Mediator;

namespace AiStudyOS.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/analytics").WithTags("Analytics");

        group.MapGet("/", async (DateOnly? from, DateOnly? to, IMediator mediator, CancellationToken ct) =>
        {
            var overview = await mediator.Send(new GetAnalyticsOverviewQuery(from, to), ct);
            return Results.Ok(overview);
        });

        group.MapGet("/dashboard", async (IMediator mediator, CancellationToken ct) =>
        {
            var dashboard = await mediator.Send(new GetDashboardAnalyticsQuery(), ct);
            return Results.Ok(dashboard);
        });

        group.MapGet("/weekly", async (IMediator mediator, CancellationToken ct) =>
        {
            var weekly = await mediator.Send(new GetWeeklyAnalyticsQuery(), ct);
            return Results.Ok(weekly);
        });

        group.MapGet("/monthly", async (IMediator mediator, CancellationToken ct) =>
        {
            var monthly = await mediator.Send(new GetMonthlyAnalyticsQuery(), ct);
            return Results.Ok(monthly);
        });

        group.MapGet("/streak", async (IMediator mediator, CancellationToken ct) =>
        {
            var streak = await mediator.Send(new GetStreakAnalyticsQuery(), ct);
            return Results.Ok(streak);
        });

        group.MapGet("/goals", async (IMediator mediator, CancellationToken ct) =>
        {
            var goals = await mediator.Send(new GetGoalAnalyticsQuery(), ct);
            return Results.Ok(goals);
        });

        group.MapGet("/quizzes", async (IMediator mediator, CancellationToken ct) =>
        {
            var quizzes = await mediator.Send(new GetQuizAnalyticsQuery(), ct);
            return Results.Ok(quizzes);
        });

        group.MapGet("/mastery", async (IMediator mediator, CancellationToken ct) =>
        {
            var mastery = await mediator.Send(new GetMasteryAnalyticsQuery(), ct);
            return Results.Ok(mastery);
        });

        group.MapGet("/planner", async (IMediator mediator, CancellationToken ct) =>
        {
            var planner = await mediator.Send(new GetPlannerAnalyticsQuery(), ct);
            return Results.Ok(planner);
        });

        group.MapGet("/mentor", async (IMediator mediator, CancellationToken ct) =>
        {
            var mentor = await mediator.Send(new GetMentorAnalyticsQuery(), ct);
            return Results.Ok(mentor);
        });

        group.MapPost("/insights/regenerate", async (IMediator mediator, CancellationToken ct) =>
        {
            var insights = await mediator.Send(new RegenerateInsightsCommand(), ct);
            return Results.Ok(insights);
        });

        group.MapGet("/export/pdf", async (DateOnly? from, DateOnly? to, IMediator mediator, CancellationToken ct) =>
        {
            var bytes = await mediator.Send(new ExportAnalyticsPdfQuery(from, to), ct);
            return Results.File(bytes, "application/pdf", "analytics-report.pdf");
        });

        group.MapGet("/export/csv", async (DateOnly? from, DateOnly? to, IMediator mediator, CancellationToken ct) =>
        {
            var bytes = await mediator.Send(new ExportAnalyticsCsvQuery(from, to), ct);
            return Results.File(bytes, "text/csv", "analytics-report.csv");
        });

        return app;
    }
}
