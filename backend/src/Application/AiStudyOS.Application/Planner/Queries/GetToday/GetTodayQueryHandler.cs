using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Planner.Commands.GenerateDailyRecommendation;
using AiStudyOS.Application.Planner.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiStudyOS.Application.Planner.Queries.GetToday;

public class GetTodayQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator,
    ILogger<GetTodayQueryHandler> logger) : IQueryHandler<GetTodayQuery, TodayPlanDto>
{
    public async ValueTask<TodayPlanDto> Handle(GetTodayQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var now = dateTimeProvider.UtcNow;

        var existing = await db.PlannerRecommendations
            .Where(r => r.UserId == userId && r.Date == today)
            .OrderByDescending(r => r.GeneratedAt)
            .FirstOrDefaultAsync(ct);

        // Reuse the cached recommendation if it's still active — not expired, not invalidated by a
        // goal/task change since it was generated. Only regenerate when necessary (§2).
        if (existing is not null && existing.IsActive(now))
            return await PlannerQueryHelpers.BuildTodayPlanAsync(db, userId, today, existing, ct);

        // No active recommendation — generate one. Tasks/deadlines/goals must keep working even if
        // this fails: only recommendation generation is allowed to degrade (§7).
        try
        {
            return await mediator.Send(new GenerateDailyRecommendationCommand(), ct);
        }
        catch (AiGenerationFailedException ex)
        {
            logger.LogWarning(ex, "Recommendation generation failed for user {UserId} on {Date}; serving plan without a recommendation", userId, today);
            return await PlannerQueryHelpers.BuildTodayPlanAsync(db, userId, today, null, ct);
        }
    }
}
