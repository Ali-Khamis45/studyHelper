using AiStudyOS.Domain.Planner;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Planner;

public class DailyTaskTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    private static DailyTask CreateTask() =>
        DailyTask.Create(UserId, goalId: null, "Study", reasoning: null, Today, estimatedMinutes: 30, TaskSource.Manual, Now);

    [Fact]
    public void Create_starts_pending_with_zero_reschedule_count()
    {
        var task = CreateTask();

        task.Status.Should().Be(DailyTaskStatus.Pending);
        task.RescheduleCount.Should().Be(0);
    }

    [Fact]
    public void Reschedule_increments_reschedule_count_and_moves_date()
    {
        var task = CreateTask();
        var newDate = Today.AddDays(3);

        task.Reschedule(newDate, Now.AddHours(1));

        task.Date.Should().Be(newDate);
        task.RescheduleCount.Should().Be(1);
        task.Status.Should().Be(DailyTaskStatus.Pending);
    }

    [Fact]
    public void Reschedule_multiple_times_accumulates_the_count()
    {
        var task = CreateTask();

        task.Reschedule(Today.AddDays(1), Now.AddHours(1));
        task.Reschedule(Today.AddDays(2), Now.AddHours(2));
        task.Reschedule(Today.AddDays(3), Now.AddHours(3));

        task.RescheduleCount.Should().Be(3);
    }

    [Fact]
    public void Reschedule_clears_completion_state()
    {
        var task = CreateTask();
        task.Complete(Now);

        task.Reschedule(Today.AddDays(1), Now.AddHours(1));

        task.Status.Should().Be(DailyTaskStatus.Pending);
        task.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Complete_then_skip_does_not_affect_reschedule_count()
    {
        var task = CreateTask();

        task.Complete(Now);
        task.Skip(Now.AddHours(1));

        task.RescheduleCount.Should().Be(0);
    }
}
