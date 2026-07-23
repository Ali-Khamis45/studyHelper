namespace AiStudyOS.Application.Common.Options;

public class MentorOptions
{
    public const string SectionName = "Mentor";

    public int MessageMaxLength { get; init; } = 8000;

    /// <summary>How many most-recent messages are rendered into the conversation-history context fragment.</summary>
    public int ConversationHistoryMessages { get; init; } = 20;

    /// <summary>Default page size for conversation and message listings.</summary>
    public int DefaultPageSize { get; init; } = 20;

    /// <summary>How many memory records MemoryContextProvider pulls in, ordered by salience then recency.</summary>
    public int MemoryContextTake { get; init; } = 8;

    /// <summary>An exchange shorter than this (user message length, chars) is considered too trivial to persist as a memory.</summary>
    public int MemoryWriteMinContentLength { get; init; } = 40;
}
