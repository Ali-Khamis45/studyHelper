using AiStudyOS.Application.Mentor.Dtos;

namespace AiStudyOS.Application.Mentor.Streaming;

public abstract record MentorStreamEvent;

public record MentorDeltaEvent(string Content) : MentorStreamEvent;

public record MentorCompleteEvent(ConversationMessageDto Message) : MentorStreamEvent;

public record MentorErrorEvent(string Message) : MentorStreamEvent;

/// <summary>
/// Drives the same Supervisor -> Intent Classifier -> Agent Registry -> Context Builder -> Prompt
/// Library -> IAiKernel pipeline as SendMessageCommandHandler, but through
/// IAiKernel.ExecuteStreamAsync so the frontend can render tokens as they arrive. The user's message
/// is persisted before streaming begins; the assistant's reply is persisted only after a successful
/// completion, so a cancelled or failed stream never leaves a partial reply in the conversation.
/// </summary>
public interface IMentorMessageStreamer
{
    IAsyncEnumerable<MentorStreamEvent> StreamMessageAsync(Guid conversationId, string content, CancellationToken ct);
}
