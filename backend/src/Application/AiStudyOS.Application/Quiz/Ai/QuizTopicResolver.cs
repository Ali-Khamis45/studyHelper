using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Domain.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Ai;

/// <summary>Shared by GenerateQuizCommandHandler and QuizGenerationStreamer: a Review-type quiz has no caller-supplied topic — it's derived from the student's current weakest topics instead.</summary>
public static class QuizTopicResolver
{
    public static async Task<string> ResolveAsync(IApplicationDbContext db, IOptions<QuizOptions> options, Guid userId, string? requestedTopic, QuizType quizType, CancellationToken ct)
    {
        if (quizType != QuizType.Review)
            return requestedTopic!;

        var weakTopics = await db.TopicMasteries
            .Where(m => m.UserId == userId && m.MasteryScore < options.Value.WeakTopicMasteryThreshold)
            .OrderBy(m => m.MasteryScore)
            .Take(3)
            .Select(m => m.Topic)
            .ToListAsync(ct);

        if (weakTopics.Count == 0)
            throw new QuizPreconditionFailedException("No weak topics identified yet — complete a Standard quiz first so there's mastery data to review.");

        return string.Join(", ", weakTopics);
    }
}
