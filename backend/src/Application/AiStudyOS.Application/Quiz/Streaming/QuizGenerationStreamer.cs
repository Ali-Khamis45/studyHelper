using System.Runtime.CompilerServices;
using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Quiz.Ai;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Domain.Quiz;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Streaming;

public class QuizGenerationStreamer(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel,
    IOptions<QuizOptions> options) : IQuizGenerationStreamer
{
    public async IAsyncEnumerable<QuizGenerationStreamEvent> StreamGenerateAsync(
        string? topic, Guid? goalId, Difficulty difficulty, IReadOnlyList<QuestionType> questionTypes, int questionCount, QuizType quizType,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();
        var resolvedTopic = await QuizTopicResolver.ResolveAsync(db, options, userId, topic, quizType, ct);

        var (_, context, prompt) = await QuizPreparation.PrepareAsync(
            agentRegistry, contextBuilder, promptLibrary, userId, resolvedTopic, difficulty, questionCount, questionTypes, quizType, ct);

        var request = new KernelRequest(AgentType.Quiz, prompt, context, prompt.ExpectedJsonSchema, UserId: userId);

        KernelResult<QuizGenerationResult>? result = null;

        await foreach (var chunk in aiKernel.ExecuteStreamAsync<QuizGenerationResult>(request, ct))
        {
            if (!chunk.IsFinal)
            {
                yield return new QuizGenerationDeltaEvent(chunk.DeltaContent);
                continue;
            }

            result = chunk.Result;
        }

        if (result is null || !result.Success || result.Data is null)
        {
            var reason = result?.Errors is { Count: > 0 } errors ? string.Join("; ", errors) : "AI generation failed while streaming.";
            yield return new QuizGenerationErrorEvent(reason);
            yield break;
        }

        var quiz = await QuizFinalizer.FinalizeAsync(db, userId, goalId, quizType, result.Data, result.Telemetry, dateTimeProvider.UtcNow, ct);
        yield return new QuizGenerationCompleteEvent(quiz);
    }
}
