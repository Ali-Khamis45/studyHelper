using AiStudyOS.Domain.Analytics;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Analytics;

public class AnalyticsInsightTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);

    private static AnalyticsInsight CreateInsight(TimeSpan validFor) => AnalyticsInsight.Create(
        UserId, "weekly", "monthly", "[]", "[]", "[]", "[]", "no risk", "llama3.1", "v1", "corr-1", Now, validFor);

    [Fact]
    public void IsActive_true_when_not_expired_and_not_invalidated()
    {
        var insight = CreateInsight(TimeSpan.FromHours(24));

        insight.IsActive(Now.AddHours(1)).Should().BeTrue();
    }

    [Fact]
    public void IsActive_false_once_expired()
    {
        var insight = CreateInsight(TimeSpan.FromHours(24));

        insight.IsActive(Now.AddHours(25)).Should().BeFalse();
    }

    [Fact]
    public void IsActive_false_once_invalidated_even_if_not_expired()
    {
        var insight = CreateInsight(TimeSpan.FromHours(24));

        insight.Invalidate(Now.AddMinutes(5));

        insight.IsActive(Now.AddMinutes(10)).Should().BeFalse();
    }

    [Fact]
    public void Invalidate_is_idempotent()
    {
        var insight = CreateInsight(TimeSpan.FromHours(24));

        insight.Invalidate(Now.AddMinutes(5));
        insight.Invalidate(Now.AddMinutes(50));

        insight.InvalidatedAtUtc.Should().Be(Now.AddMinutes(5));
    }
}
