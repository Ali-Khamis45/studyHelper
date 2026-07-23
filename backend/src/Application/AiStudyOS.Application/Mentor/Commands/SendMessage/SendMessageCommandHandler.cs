using AiStudyOS.Application.AI.Agents;
using AiStudyOS.Application.AI.Context;
using AiStudyOS.Application.AI.Kernel;
using AiStudyOS.Application.AI.Prompts;
using AiStudyOS.Application.AI.Routing;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Mentor.Ai;
using AiStudyOS.Application.Mentor.Dtos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Mentor.Commands.SendMessage;

public class SendMessageCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IIntentClassifier intentClassifier,
    IAgentRegistry agentRegistry,
    IContextBuilder contextBuilder,
    IPromptLibrary promptLibrary,
    IAiKernel aiKernel,
    MentorConversationStore conversationStore) : ICommandHandler<SendMessageCommand, ConversationMessageDto>
{
    public async ValueTask<ConversationMessageDto> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == command.ConversationId && c.UserId == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Mentor.Conversation), command.ConversationId);

        var content = command.Content.Trim();
        var now = dateTimeProvider.UtcNow;

        await conversationStore.AppendUserMessageAsync(conversation, userId, content, now, ct);

        var (intent, agentDefinition, context, prompt) = await MentorOrchestrator.PrepareAsync(
            db, intentClassifier, agentRegistry, contextBuilder, promptLibrary, userId, conversation.Id, content, ct);

        var kernelResult = await aiKernel.ExecuteAsync<string>(new KernelRequest(intent, prompt, context, prompt.ExpectedJsonSchema, UserMessage: content, UserId: userId), ct);

        if (!kernelResult.Success || kernelResult.Data is null)
            throw new AiGenerationFailedException(string.Join("; ", kernelResult.Errors.DefaultIfEmpty("no response")));

        return await conversationStore.AppendAssistantMessageAsync(
            conversation, userId, kernelResult.Data, intent, kernelResult.Telemetry, agentDefinition, content, dateTimeProvider.UtcNow, ct);
    }
}
