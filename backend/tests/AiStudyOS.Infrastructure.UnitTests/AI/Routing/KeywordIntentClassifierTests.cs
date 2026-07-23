using AiStudyOS.Application.AI.Routing;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Infrastructure.AI.Routing;
using FluentAssertions;

namespace AiStudyOS.Infrastructure.UnitTests.AI.Routing;

public class KeywordIntentClassifierTests
{
    private readonly KeywordIntentClassifier _classifier = new();
    private static ConversationContext EmptyContext(Guid? conversationId = null) => new(conversationId ?? Guid.NewGuid(), Guid.NewGuid(), []);

    [Fact]
    public async Task ClassifyAsync_routes_planning_language_to_planner()
    {
        var result = await _classifier.ClassifyAsync("Can you help me schedule my tasks for today and reorganize my priorities?", EmptyContext(), CancellationToken.None);

        result.Intent.Should().Be(AgentType.Planner);
        result.Confidence.Should().BeGreaterThan(0.3);
    }

    [Fact]
    public async Task ClassifyAsync_routes_progress_language_to_analytics()
    {
        var result = await _classifier.ClassifyAsync("How is my progress this week? What's my current streak and completion rate?", EmptyContext(), CancellationToken.None);

        result.Intent.Should().Be(AgentType.Analytics);
    }

    [Fact]
    public async Task ClassifyAsync_routes_quiz_requests_to_examiner()
    {
        var result = await _classifier.ClassifyAsync("Can you quiz me with some practice questions on this topic?", EmptyContext(), CancellationToken.None);

        result.Intent.Should().Be(AgentType.Examiner);
    }

    [Fact]
    public async Task ClassifyAsync_routes_teaching_language_to_tutor()
    {
        var result = await _classifier.ClassifyAsync("Can you explain this concept and help me understand it better?", EmptyContext(), CancellationToken.None);

        result.Intent.Should().Be(AgentType.Tutor);
    }

    [Fact]
    public async Task ClassifyAsync_falls_back_to_tutor_with_low_confidence_when_no_keywords_match()
    {
        var result = await _classifier.ClassifyAsync("xyzzy plugh qwerty", EmptyContext(), CancellationToken.None);

        result.Intent.Should().Be(AgentType.Tutor);
        result.Confidence.Should().Be(0.3);
        result.MatchedRule.Should().Be("fallback");
    }

    [Fact]
    public async Task ClassifyAsync_is_case_insensitive()
    {
        var result = await _classifier.ClassifyAsync("QUIZ ME PLEASE", EmptyContext(), CancellationToken.None);

        result.Intent.Should().Be(AgentType.Examiner);
    }

    [Fact]
    public async Task ClassifyAsync_more_keyword_hits_increase_confidence()
    {
        var weak = await _classifier.ClassifyAsync("What's my progress?", EmptyContext(), CancellationToken.None);
        var strong = await _classifier.ClassifyAsync("What's my progress, streak, and completion rate? Show me analytics and stats.", EmptyContext(), CancellationToken.None);

        strong.Confidence.Should().BeGreaterThan(weak.Confidence);
    }
}
