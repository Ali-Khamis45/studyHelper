using System.Runtime.CompilerServices;
using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Routing;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Mentor.Ai;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Mentor.Streaming;

public class MentorMessageStreamer(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IIntentClassifier intentClassifier,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel,
    MentorConversationStore conversationStore) : IMentorMessageStreamer
{
    public async IAsyncEnumerable<MentorStreamEvent> StreamMessageAsync(Guid conversationId, string content, [EnumeratorCancellation] CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Mentor.Conversation), conversationId);

        var trimmedContent = content.Trim();
        await conversationStore.AppendUserMessageAsync(conversation, userId, trimmedContent, dateTimeProvider.UtcNow, ct);

        var (intent, agentDefinition, context, prompt) = await MentorOrchestrator.PrepareAsync(
            db, intentClassifier, agentRegistry, contextBuilder, promptLibrary, userId, conversation.Id, trimmedContent, ct);

        var request = new KernelRequest(intent, prompt, context, prompt.ExpectedJsonSchema, UserMessage: trimmedContent, UserId: userId);

        // AiKernel parses/validates/retries internally, identically to ExecuteAsync — this class
        // only forwards deltas as they arrive and reads the final assembled text off the last chunk.
        KernelResult<string>? result = null;

        await foreach (var chunk in aiKernel.ExecuteStreamAsync<string>(request, ct))
        {
            if (!chunk.IsFinal)
            {
                yield return new MentorDeltaEvent(chunk.DeltaContent);
                continue;
            }

            result = chunk.Result;
        }

        if (result is null || !result.Success || result.Data is null)
        {
            var reason = result?.Errors is { Count: > 0 } errors ? string.Join("; ", errors) : "AI generation failed while streaming.";
            yield return new MentorErrorEvent(reason);
            yield break;
        }

        var messageDto = await conversationStore.AppendAssistantMessageAsync(
            conversation, userId, result.Data, intent, result.Telemetry, agentDefinition, trimmedContent, dateTimeProvider.UtcNow, ct);

        yield return new MentorCompleteEvent(messageDto);
    }
}
