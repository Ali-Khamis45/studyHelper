using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AiStudyOS.Api.Endpoints;
using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Mentor.Dtos;
using AiStudyOS.Domain.Mentor;
using AiStudyOS.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiStudyOS.Api.IntegrationTests;

/// <summary>
/// Exercises the M7 Mentor module: conversation CRUD/search/pagination/pin/rename/delete (all pure
/// read/write logic, no AI involved) plus a small number of tests that go through the real
/// Supervisor -> Intent Classifier -> Agent Registry -> Context Builder -> Prompt Library -> IAiKernel
/// -> Provider -> Telemetry -> Persistence pipeline against a real Ollama instance, mirroring how
/// PlannerEndpointsTests exercises real AI generation for the Planner module.
/// </summary>
public class MentorEndpointsTests(GenerousRateLimitAuthApiFactory factory) : IClassFixture<GenerousRateLimitAuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(HttpClient Client, Guid UserId)> AuthenticatedClientAsync()
    {
        var email = $"mentor-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "correct-horse-battery", "Mentor Test"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return (client, body.User.Id);
    }

    private static async Task<ConversationDto> CreateConversationAsync(HttpClient client, string? title = null)
    {
        var response = await client.PostAsJsonAsync("/api/v1/mentor/conversations", new CreateConversationRequest(title));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ConversationDto>())!;
    }

    /// <summary>Directly inserts a user+assistant message pair, bypassing AI — legitimate for
    /// seeding history the CRUD/search/pagination tests need without depending on real generation.</summary>
    private async Task SeedMessagePairAsync(Guid conversationId, Guid userId, string userContent, string assistantContent, DateTime nowUtc)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var conversation = await db.Conversations.SingleAsync(c => c.Id == conversationId);
        var userMessage = ConversationMessage.CreateUserMessage(conversationId, userId, userContent, nowUtc);
        var assistantMessage = ConversationMessage.CreateAssistantMessage(conversationId, userId, assistantContent, AgentType.Tutor, "test-model", 50, 80, "test-correlation", nowUtc.AddSeconds(1));

        db.ConversationMessages.AddRange(userMessage, assistantMessage);
        conversation.RecordExchange(50, 80, nowUtc.AddSeconds(1));
        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------------------------------
    // Conversation lifecycle: create / rename / pin / delete
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task CreateConversation_WithNoTitle_UsesDefaultTitle()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var conversation = await CreateConversationAsync(client);

        conversation.Title.Should().Be(Conversation.DefaultTitle);
        conversation.IsPinned.Should().BeFalse();
        conversation.MessageCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateConversation_WithTitle_UsesProvidedTitle()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var conversation = await CreateConversationAsync(client, "Exam prep chat");

        conversation.Title.Should().Be("Exam prep chat");
    }

    [Fact]
    public async Task RenameConversation_UpdatesTitle()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        var response = await client.PatchAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/rename", new RenameConversationRequest("New Title"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var renamed = await response.Content.ReadFromJsonAsync<ConversationDto>();
        renamed!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task RenameConversation_EmptyTitle_ReturnsBadRequest()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        var response = await client.PatchAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/rename", new RenameConversationRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenameConversation_NotOwner_ReturnsNotFound()
    {
        var (ownerClient, _) = await AuthenticatedClientAsync();
        var (otherClient, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(ownerClient);

        var response = await otherClient.PatchAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/rename", new RenameConversationRequest("Hijacked"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetConversationPinned_TogglesPin()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        var pinResponse = await client.PatchAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/pin", new SetConversationPinnedRequest(true));
        (await pinResponse.Content.ReadFromJsonAsync<ConversationDto>())!.IsPinned.Should().BeTrue();

        var unpinResponse = await client.PatchAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/pin", new SetConversationPinnedRequest(false));
        (await unpinResponse.Content.ReadFromJsonAsync<ConversationDto>())!.IsPinned.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteConversation_RemovesConversationAndCascadesMessages()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);
        await SeedMessagePairAsync(conversation.Id, userId, "Hello", "Hi there", DateTime.UtcNow);

        var deleteResponse = await client.DeleteAsync($"/api/v1/mentor/conversations/{conversation.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/mentor/conversations/{conversation.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingMessages = await db.ConversationMessages.CountAsync(m => m.ConversationId == conversation.Id);
        remainingMessages.Should().Be(0);
    }

    [Fact]
    public async Task DeleteConversation_AlreadyDeleted_ReturnsNotFound()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);
        await client.DeleteAsync($"/api/v1/mentor/conversations/{conversation.Id}");

        var response = await client.DeleteAsync($"/api/v1/mentor/conversations/{conversation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------------------------------
    // Listing: search / pinned filter / pagination / user isolation
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetConversations_OnlyReturnsOwnConversations()
    {
        var (clientA, _) = await AuthenticatedClientAsync();
        var (clientB, _) = await AuthenticatedClientAsync();
        await CreateConversationAsync(clientA, "A's chat");
        await CreateConversationAsync(clientB, "B's chat");

        var response = await clientA.GetAsync("/api/v1/mentor/conversations");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ConversationDto>>();

        result!.Items.Should().ContainSingle(c => c.Title == "A's chat");
        result.Items.Should().NotContain(c => c.Title == "B's chat");
    }

    [Fact]
    public async Task GetConversations_Search_FiltersByTitleCaseInsensitively()
    {
        var (client, _) = await AuthenticatedClientAsync();
        await CreateConversationAsync(client, "Calculus review session");
        await CreateConversationAsync(client, "Career advice chat");

        var response = await client.GetAsync("/api/v1/mentor/conversations?search=CALCULUS");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ConversationDto>>();

        result!.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Calculus review session");
    }

    [Fact]
    public async Task GetConversations_PinnedOnly_ReturnsOnlyPinned()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var pinned = await CreateConversationAsync(client, "Pinned chat");
        await CreateConversationAsync(client, "Unpinned chat");
        await client.PatchAsJsonAsync($"/api/v1/mentor/conversations/{pinned.Id}/pin", new SetConversationPinnedRequest(true));

        var response = await client.GetAsync("/api/v1/mentor/conversations?pinned=true");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ConversationDto>>();

        result!.Items.Should().ContainSingle(c => c.Title == "Pinned chat");
    }

    [Fact]
    public async Task GetConversations_Pagination_ReturnsCorrectPageAndTotal()
    {
        var (client, _) = await AuthenticatedClientAsync();
        for (var i = 0; i < 5; i++)
            await CreateConversationAsync(client, $"Chat {i}");

        var page1 = await (await client.GetAsync("/api/v1/mentor/conversations?page=1&pageSize=2")).Content.ReadFromJsonAsync<PagedResult<ConversationDto>>();
        var page2 = await (await client.GetAsync("/api/v1/mentor/conversations?page=2&pageSize=2")).Content.ReadFromJsonAsync<PagedResult<ConversationDto>>();

        page1!.Items.Should().HaveCount(2);
        page2!.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.TotalPages.Should().Be(3);
        page1.Items.Select(c => c.Id).Should().NotIntersectWith(page2.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task GetConversations_PinnedConversationsSortFirst()
    {
        var (client, _) = await AuthenticatedClientAsync();
        await CreateConversationAsync(client, "Regular");
        var pinned = await CreateConversationAsync(client, "Important");
        await client.PatchAsJsonAsync($"/api/v1/mentor/conversations/{pinned.Id}/pin", new SetConversationPinnedRequest(true));

        var result = await (await client.GetAsync("/api/v1/mentor/conversations")).Content.ReadFromJsonAsync<PagedResult<ConversationDto>>();

        result!.Items[0].Title.Should().Be("Important");
    }

    // ---------------------------------------------------------------------------------------
    // Messages: pagination / ordering
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetConversationMessages_ReturnsInAscendingOrder()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);
        var now = DateTime.UtcNow;
        await SeedMessagePairAsync(conversation.Id, userId, "First question", "First answer", now);
        await SeedMessagePairAsync(conversation.Id, userId, "Second question", "Second answer", now.AddMinutes(1));

        var response = await client.GetAsync($"/api/v1/mentor/conversations/{conversation.Id}/messages");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ConversationMessageDto>>();

        result!.Items.Should().HaveCount(4);
        result.Items[0].Content.Should().Be("First question");
        result.Items[^1].Content.Should().Be("Second answer");
    }

    [Fact]
    public async Task GetConversationMessages_Pagination_NewestPageFirstButAscendingWithinPage()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);
        var now = DateTime.UtcNow;
        for (var i = 0; i < 3; i++)
            await SeedMessagePairAsync(conversation.Id, userId, $"Q{i}", $"A{i}", now.AddMinutes(i));

        var page1 = await (await client.GetAsync($"/api/v1/mentor/conversations/{conversation.Id}/messages?page=1&pageSize=2")).Content.ReadFromJsonAsync<PagedResult<ConversationMessageDto>>();

        page1!.TotalCount.Should().Be(6);
        page1.Items.Should().HaveCount(2);
        // Page 1 = the two most recent messages, still ascending within the page.
        page1.Items[0].Content.Should().Be("Q2");
        page1.Items[1].Content.Should().Be("A2");
    }

    [Fact]
    public async Task GetConversationMessages_NotOwner_ReturnsNotFound()
    {
        var (ownerClient, ownerId) = await AuthenticatedClientAsync();
        var (otherClient, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(ownerClient);
        await SeedMessagePairAsync(conversation.Id, ownerId, "Private", "Reply", DateTime.UtcNow);

        var response = await otherClient.GetAsync($"/api/v1/mentor/conversations/{conversation.Id}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------------------------------
    // Real AI: send + stream (exercises the full Supervisor -> ... -> Persistence pipeline)
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task SendMessage_RealAi_PersistsExchangeAndUpdatesConversationStats()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        var response = await client.PostAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/messages", new SendMessageRequest("Give me one specific tip for staying focused while studying."));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var assistantMessage = await response.Content.ReadFromJsonAsync<ConversationMessageDto>();
        assistantMessage!.Role.Should().Be(nameof(MessageRole.Assistant));
        assistantMessage.Content.Should().NotBeNullOrWhiteSpace();
        assistantMessage.AgentType.Should().NotBeNullOrEmpty();
        assistantMessage.ModelUsed.Should().NotBeNullOrEmpty();

        var updatedConversation = await (await client.GetAsync($"/api/v1/mentor/conversations/{conversation.Id}")).Content.ReadFromJsonAsync<ConversationDto>();
        updatedConversation!.MessageCount.Should().Be(2);
        updatedConversation.TotalCompletionTokens.Should().BeGreaterThan(0);
        // First exchange on a default-titled conversation auto-derives a real title from the message.
        updatedConversation.Title.Should().NotBe(Conversation.DefaultTitle);
    }

    [Fact]
    public async Task SendMessage_ConversationNotFound_ReturnsNotFound()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync($"/api/v1/mentor/conversations/{Guid.NewGuid()}/messages", new SendMessageRequest("Hello"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendMessage_EmptyContent_ReturnsBadRequest()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        var response = await client.PostAsJsonAsync($"/api/v1/mentor/conversations/{conversation.Id}/messages", new SendMessageRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StreamMessage_RealAi_ReturnsDeltaAndCompleteEvents_AndPersists()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        var events = await StreamAsync(client, conversation.Id, "In one short sentence, what is spaced repetition?");

        events.Should().Contain(e => e.Type == "delta");
        events.Should().ContainSingle(e => e.Type == "complete");
        events.Should().NotContain(e => e.Type == "error");

        var fullText = string.Concat(events.Where(e => e.Type == "delta").Select(e => e.Content));
        fullText.Should().NotBeNullOrWhiteSpace();

        var messages = await (await client.GetAsync($"/api/v1/mentor/conversations/{conversation.Id}/messages")).Content.ReadFromJsonAsync<PagedResult<ConversationMessageDto>>();
        messages!.Items.Should().HaveCount(2);
        messages.Items[0].Role.Should().Be(nameof(MessageRole.User));
        messages.Items[1].Role.Should().Be(nameof(MessageRole.Assistant));
    }

    [Fact]
    public async Task StreamMessage_CancelledMidStream_LeavesConversationConsistent()
    {
        var (client, _) = await AuthenticatedClientAsync();
        var conversation = await CreateConversationAsync(client);

        using var cts = new CancellationTokenSource();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/mentor/conversations/{conversation.Id}/messages/stream")
        {
            Content = JsonContent.Create(new SendMessageRequest("Write a long, detailed explanation of the entire history of calculus."))
        };

        // ResponseHeadersRead alone isn't enough to observe cancellation — headers arrive almost
        // immediately (before any AI output exists). The token has to also cancel the body read,
        // which is where the real, still-in-progress generation is waited on.
        var act = async () =>
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            while (await reader.ReadLineAsync(cts.Token) is not null) { }
        };

        cts.CancelAfter(TimeSpan.FromMilliseconds(300));
        // TestServer's in-memory transport surfaces a client-side cancellation as IOException
        // ("The client aborted the request") rather than OperationCanceledException — a real socket
        // would throw the latter, but either way the request never completed normally, which is
        // what actually matters here; the DB assertion below is the real correctness check.
        await act.Should().ThrowAsync<Exception>();

        // The user's message must survive the cancellation; a partial/incomplete assistant reply
        // must never be persisted — only a full, successfully-completed stream is saved (mirrors
        // RecommendationStreamer's identical guarantee for the Planner module).
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var messages = await db.ConversationMessages.Where(m => m.ConversationId == conversation.Id).ToListAsync();

        messages.Should().ContainSingle();
        messages[0].Role.Should().Be(MessageRole.User);
    }

    private static async Task<List<(string Type, string? Content)>> StreamAsync(HttpClient client, Guid conversationId, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/mentor/conversations/{conversationId}/messages/stream")
        {
            Content = JsonContent.Create(new SendMessageRequest(content))
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var events = new List<(string Type, string? Content)>();
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            using var doc = JsonDocument.Parse(line);
            var type = doc.RootElement.GetProperty("type").GetString()!;
            var contentValue = doc.RootElement.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null;
            events.Add((type, contentValue));
        }

        return events;
    }
}
