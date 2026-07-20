using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Planner.Dtos;
using AiStudyOS.Domain.Planner;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Planner.Queries.GetWeek;

public class GetWeekQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider dateTimeProvider, IOptions<PlannerOptions> plannerOptions) : IQueryHandler<GetWeekQuery, WeekDto>
{
    public async ValueTask<WeekDto> Handle(GetWeekQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var options = plannerOptions.Value;
        var rangeEnd = today.AddDays(options.WeekViewDays - 1);

        var tasks = await db.DailyTasks
            .Where(t => t.UserId == userId && t.Date >= today && t.Date <= rangeEnd)
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        var goalTitles = await PlannerQueryHelpers.GetGoalTitlesAsync(db, userId, tasks.Select(t => t.GoalId), ct);

        var days = Enumerable.Range(0, options.WeekViewDays)
            .Select(offset =>
            {
                var date = today.AddDays(offset);
                var dayTasks = tasks.Where(t => t.Date == date).ToList();
                var dayTaskDtos = dayTasks
                    .Select(t => DailyTaskDto.FromDomain(t, t.GoalId is { } id ? goalTitles.GetValueOrDefault(id) : null, today))
                    .ToList();
                var totalMinutes = dayTasks.Sum(t => t.EstimatedMinutes);

                return new WeekDayDto(date, dayTaskDtos, totalMinutes, totalMinutes > options.DailyWorkloadThresholdMinutes);
            })
            .ToList();

        var weeklyCompletionPercent = tasks.Count == 0
            ? 0
            : Math.Round(100.0 * tasks.Count(t => t.Status == DailyTaskStatus.Completed) / tasks.Count, 1);

        return new WeekDto(days, weeklyCompletionPercent);
    }
}
