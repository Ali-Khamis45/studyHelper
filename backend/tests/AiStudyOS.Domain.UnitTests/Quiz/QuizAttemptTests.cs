using AiStudyOS.Domain.Quiz;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Quiz;

public class QuizAttemptTests
{
    private static readonly Guid QuizId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_starts_in_progress_with_no_score()
    {
        var attempt = QuizAttempt.Create(QuizId, UserId, Now);

        attempt.Status.Should().Be(AttemptStatus.InProgress);
        attempt.Score.Should().BeNull();
        attempt.CompletedAtUtc.Should().BeNull();
        attempt.StartedAtUtc.Should().Be(Now);
    }

    [Theory]
    [InlineData(10, 10, 100.0)]
    [InlineData(0, 10, 0.0)]
    [InlineData(3, 4, 75.0)]
    [InlineData(1, 3, 33.3)]
    public void Complete_computes_percentage_score(int correct, int total, double expectedScore)
    {
        var attempt = QuizAttempt.Create(QuizId, UserId, Now);
        var completedAt = Now.AddMinutes(5);

        attempt.Complete(correct, total, completedAt);

        attempt.Status.Should().Be(AttemptStatus.Completed);
        attempt.Score.Should().Be(expectedScore);
        attempt.CorrectCount.Should().Be(correct);
        attempt.TotalCount.Should().Be(total);
        attempt.CompletedAtUtc.Should().Be(completedAt);
    }

    [Fact]
    public void Complete_with_zero_questions_scores_zero_not_a_divide_by_zero_error()
    {
        var attempt = QuizAttempt.Create(QuizId, UserId, Now);

        attempt.Complete(0, 0, Now.AddMinutes(1));

        attempt.Score.Should().Be(0);
    }
}
