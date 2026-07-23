using AiStudyOS.Domain.Quiz;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Quiz;

public class TopicMasteryTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_sets_initial_score_and_one_attempt()
    {
        var mastery = TopicMastery.Create(UserId, "Algebra", 0.7, Now);

        mastery.Topic.Should().Be("Algebra");
        mastery.MasteryScore.Should().Be(0.7);
        mastery.AttemptsCount.Should().Be(1);
        mastery.LastUpdatedUtc.Should().Be(Now);
    }

    [Fact]
    public void Create_clamps_initial_score_to_0_1_range()
    {
        TopicMastery.Create(UserId, "Algebra", 1.5, Now).MasteryScore.Should().Be(1);
        TopicMastery.Create(UserId, "Algebra", -0.5, Now).MasteryScore.Should().Be(0);
    }

    [Fact]
    public void ApplyQuizResult_applies_weighted_moving_average_with_default_alpha()
    {
        var mastery = TopicMastery.Create(UserId, "Algebra", 0.5, Now);
        var later = Now.AddDays(1);

        // alpha=0.3: new = 0.5*(1-0.3) + 1.0*0.3 = 0.35 + 0.3 = 0.65
        mastery.ApplyQuizResult(topicScore: 1.0, later);

        mastery.MasteryScore.Should().BeApproximately(0.65, 0.0001);
        mastery.AttemptsCount.Should().Be(2);
        mastery.LastUpdatedUtc.Should().Be(later);
    }

    [Fact]
    public void ApplyQuizResult_a_perfect_topic_score_increases_mastery_toward_1()
    {
        var mastery = TopicMastery.Create(UserId, "Algebra", 0.5, Now);

        for (var i = 0; i < 20; i++)
            mastery.ApplyQuizResult(1.0, Now.AddDays(i + 1));

        mastery.MasteryScore.Should().BeGreaterThan(0.99);
    }

    [Fact]
    public void ApplyQuizResult_a_zero_topic_score_decreases_mastery_toward_0()
    {
        var mastery = TopicMastery.Create(UserId, "Algebra", 0.5, Now);

        for (var i = 0; i < 20; i++)
            mastery.ApplyQuizResult(0.0, Now.AddDays(i + 1));

        mastery.MasteryScore.Should().BeLessThan(0.01);
    }

    [Fact]
    public void ApplyQuizResult_respects_a_custom_alpha()
    {
        var mastery = TopicMastery.Create(UserId, "Algebra", 0.5, Now);

        // alpha=1.0 means the new score fully replaces the old one.
        mastery.ApplyQuizResult(0.9, Now.AddDays(1), alpha: 1.0);

        mastery.MasteryScore.Should().Be(0.9);
    }

    [Fact]
    public void ApplyQuizResult_stays_clamped_within_0_1()
    {
        var mastery = TopicMastery.Create(UserId, "Algebra", 0.95, Now);

        mastery.ApplyQuizResult(1.5, Now.AddDays(1));

        mastery.MasteryScore.Should().BeLessThanOrEqualTo(1.0);
    }
}
