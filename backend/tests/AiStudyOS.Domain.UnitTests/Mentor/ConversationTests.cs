using AiStudyOS.Domain.Mentor;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Mentor;

public class ConversationTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_no_title_uses_default_title()
    {
        var conversation = Conversation.Create(UserId, title: null, Now);

        conversation.Title.Should().Be(Conversation.DefaultTitle);
        conversation.HasDefaultTitle.Should().BeTrue();
        conversation.IsPinned.Should().BeFalse();
        conversation.MessageCount.Should().Be(0);
    }

    [Fact]
    public void Create_with_title_trims_and_keeps_it()
    {
        var conversation = Conversation.Create(UserId, "  My Chat  ", Now);

        conversation.Title.Should().Be("My Chat");
        conversation.HasDefaultTitle.Should().BeFalse();
    }

    [Fact]
    public void Rename_updates_title_and_timestamp()
    {
        var conversation = Conversation.Create(UserId, null, Now);
        var later = Now.AddMinutes(5);

        conversation.Rename("Renamed", later);

        conversation.Title.Should().Be("Renamed");
        conversation.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void SetPinned_toggles_flag()
    {
        var conversation = Conversation.Create(UserId, null, Now);

        conversation.SetPinned(true, Now);
        conversation.IsPinned.Should().BeTrue();

        conversation.SetPinned(false, Now);
        conversation.IsPinned.Should().BeFalse();
    }

    [Fact]
    public void RecordExchange_increments_counts_and_sets_last_message_time()
    {
        var conversation = Conversation.Create(UserId, null, Now);
        var messageTime = Now.AddMinutes(1);

        conversation.RecordExchange(promptTokens: 120, completionTokens: 340, messageTime);

        conversation.MessageCount.Should().Be(2);
        conversation.TotalPromptTokens.Should().Be(120);
        conversation.TotalCompletionTokens.Should().Be(340);
        conversation.LastMessageAtUtc.Should().Be(messageTime);
        conversation.UpdatedAtUtc.Should().Be(messageTime);
    }

    [Fact]
    public void RecordExchange_accumulates_across_multiple_calls()
    {
        var conversation = Conversation.Create(UserId, null, Now);

        conversation.RecordExchange(100, 200, Now.AddMinutes(1));
        conversation.RecordExchange(50, 75, Now.AddMinutes(2));

        conversation.MessageCount.Should().Be(4);
        conversation.TotalPromptTokens.Should().Be(150);
        conversation.TotalCompletionTokens.Should().Be(275);
    }
}
