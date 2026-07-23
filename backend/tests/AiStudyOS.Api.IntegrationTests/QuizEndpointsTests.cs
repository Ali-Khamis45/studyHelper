using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AiStudyOS.Api.Endpoints;
using AiStudyOS.Application.Common.Dtos;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Quiz.Commands.SubmitQuiz;
using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Domain.Quiz;
using AiStudyOS.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiStudyOS.Api.IntegrationTests;

/// <summary>
/// Exercises the M8 Quiz module: generation (real AI, sync + streaming), grading, topic-mastery
/// updates, weak-topic detection, history, retry, delete, and authorization/isolation — mirrors
/// MentorEndpointsTests' structure (same real-AI-where-it-matters, seed-directly-otherwise approach).
/// </summary>
public class QuizEndpointsTests(GenerousRateLimitAuthApiFactory factory) : IClassFixture<GenerousRateLimitAuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(HttpClient Client, Guid UserId)> AuthenticatedClientAsync()
    {
        var email = $"quiz-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "correct-horse-battery", "Quiz Test"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return (client, body.User.Id);
    }

    /// <summary>Directly inserts a Quiz + QuizQuestions, bypassing AI generation — legitimate for tests that exercise submission/grading/history rather than generation itself.</summary>
    private async Task<Domain.Quiz.Quiz> SeedQuizAsync(Guid userId, string topic, Difficulty difficulty, params (QuestionType Type, string Topic, string Text, string? OptionsJson, string CorrectAnswer)[] questions)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var quiz = Domain.Quiz.Quiz.Create(userId, null, "Seeded Quiz", topic, difficulty, QuizType.Standard, questions.Length, "test-model", "v1", "test-correlation", DateTime.UtcNow);
        db.Quizzes.Add(quiz);

        var order = 0;
        foreach (var q in questions)
        {
            db.QuizQuestions.Add(QuizQuestion.Create(quiz.Id, order++, q.Type, q.Topic, difficulty, q.Text, q.OptionsJson, q.CorrectAnswer, $"Explanation for: {q.Text}"));
        }

        await db.SaveChangesAsync();
        return quiz;
    }

    // ---------------------------------------------------------------------------------------
    // Real AI: generate (sync + stream)
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GenerateQuiz_RealAi_PersistsQuizWithQuestions()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/generate", new GenerateQuizRequest(
            "Basic Algebra", null, Difficulty.Easy, [QuestionType.MultipleChoice, QuestionType.TrueFalse], 3, QuizType.Standard));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var quiz = await response.Content.ReadFromJsonAsync<QuizDto>();

        quiz!.Questions.Should().HaveCount(3);
        quiz.Questions.Should().OnlyContain(q => q.Type == "MultipleChoice" || q.Type == "TrueFalse");
        // QuestionDto must never leak the answer key before submission — verified structurally
        // (the type has no such property at all), not just "the field happened to be empty".
        typeof(QuestionDto).GetProperty("CorrectAnswer").Should().BeNull();
        typeof(QuestionDto).GetProperty("Explanation").Should().BeNull();
    }

    [Fact]
    public async Task GenerateQuiz_InvalidQuestionCount_ReturnsBadRequest()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/generate", new GenerateQuizRequest(
            "Algebra", null, Difficulty.Easy, [QuestionType.MultipleChoice], 999, QuizType.Standard));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateQuiz_ReviewType_WithNoMasteryHistory_ReturnsBadRequest()
    {
        var (client, _) = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/generate", new GenerateQuizRequest(
            null, null, Difficulty.Easy, [QuestionType.MultipleChoice], 3, QuizType.Review));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StreamGenerateQuiz_RealAi_ReturnsDeltaAndCompleteEvents_AndPersists()
    {
        var (client, _) = await AuthenticatedClientAsync();

        // A real, unmocked 8B model occasionally omits a required field (caught cleanly by
        // QuizFinalizer.RequireField as a 503 — the intended, correct behavior, not a bug) rather
        // than producing malformed JSON that AiKernel's own repair-retry would catch. One test-level
        // retry mirrors what a real user would do — click "generate" again — without weakening the
        // assertion that a SUCCESSFUL response must be fully well-formed.
        List<(string Type, string? Content)> events = [];
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            events = await StreamGenerateAsync(client, "Basic Geography", Difficulty.Easy, [QuestionType.ShortAnswer], 2, QuizType.Standard);
            if (events.All(e => e.Type != "error")) break;
        }

        events.Should().Contain(e => e.Type == "delta");
        events.Should().ContainSingle(e => e.Type == "complete");
        events.Should().NotContain(e => e.Type == "error");
    }

    [Fact]
    public async Task StreamGenerateQuiz_CancelledMidStream_NoQuizPersisted()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        using var cts = new CancellationTokenSource();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/quiz/generate/stream")
        {
            Content = JsonContent.Create(new GenerateQuizRequest("A very long and detailed history of the entire Roman Empire", null, Difficulty.Hard, [QuestionType.ShortAnswer], 10, QuizType.Standard))
        };

        var act = async () =>
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            while (await reader.ReadLineAsync(cts.Token) is not null) { }
        };

        cts.CancelAfter(TimeSpan.FromMilliseconds(300));
        await act.Should().ThrowAsync<Exception>();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var quizCount = await db.Quizzes.CountAsync(q => q.UserId == userId);

        // A cancelled generation must never leave a partial quiz behind — persistence only happens
        // once, after the full result is parsed successfully (mirrors Mentor/Planner streaming).
        quizCount.Should().Be(0);
    }

    private static async Task<List<(string Type, string? Content)>> StreamGenerateAsync(HttpClient client, string topic, Difficulty difficulty, IReadOnlyList<QuestionType> types, int count, QuizType quizType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/quiz/generate/stream")
        {
            Content = JsonContent.Create(new GenerateQuizRequest(topic, null, difficulty, types, count, quizType))
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
            var content = doc.RootElement.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null;
            events.Add((type, content));
        }

        return events;
    }

    // ---------------------------------------------------------------------------------------
    // Read: single / list / isolation
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetQuiz_NotOwner_ReturnsNotFound()
    {
        var (ownerClient, ownerId) = await AuthenticatedClientAsync();
        var (otherClient, _) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(ownerId, "Algebra", Difficulty.Easy, (QuestionType.MultipleChoice, "Algebra", "2+2=?", "[\"3\",\"4\",\"5\"]", "4"));
        _ = ownerClient;

        var response = await otherClient.GetAsync($"/api/v1/quiz/{quiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuizzes_OnlyReturnsOwnQuizzes()
    {
        var (clientA, userIdA) = await AuthenticatedClientAsync();
        var (_, userIdB) = await AuthenticatedClientAsync();
        await SeedQuizAsync(userIdA, "A's topic", Difficulty.Easy, (QuestionType.MultipleChoice, "A's topic", "Q?", "[\"1\",\"2\"]", "1"));
        await SeedQuizAsync(userIdB, "B's topic", Difficulty.Easy, (QuestionType.MultipleChoice, "B's topic", "Q?", "[\"1\",\"2\"]", "1"));

        var response = await clientA.GetAsync("/api/v1/quiz");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<QuizSummaryDto>>();

        result!.Items.Should().ContainSingle(q => q.Topic == "A's topic");
        result.Items.Should().NotContain(q => q.Topic == "B's topic");
    }

    // ---------------------------------------------------------------------------------------
    // Submit / grading / mastery
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task SubmitQuiz_GradesAnswersCorrectlyAndComputesScore()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "Arithmetic", Difficulty.Easy,
            (QuestionType.MultipleChoice, "Arithmetic", "2+2=?", "[\"3\",\"4\",\"5\"]", "4"),
            (QuestionType.TrueFalse, "Arithmetic", "5>3", "[\"True\",\"False\"]", "True"),
            (QuestionType.FillBlank, "Arithmetic", "3+3=____", null, "6"));

        var questions = await GetQuestionsAsync(quiz.Id);
        var answers = new[]
        {
            new SubmittedAnswer(questions[0].Id, "4"),   // correct
            new SubmittedAnswer(questions[1].Id, "False"), // incorrect
            new SubmittedAnswer(questions[2].Id, "6"),   // correct
        };

        var response = await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz.Id, answers));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<QuizAttemptResultDto>();
        result!.CorrectCount.Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.Score.Should().BeApproximately(66.7, 0.1);
        result.Answers.Should().HaveCount(3);
        result.Answers.Single(a => a.QuestionId == questions[1].Id).IsCorrect.Should().BeFalse();
        result.Answers.Single(a => a.QuestionId == questions[1].Id).Explanation.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SubmitQuiz_CreatesInitialTopicMastery()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "Chemistry", Difficulty.Medium,
            (QuestionType.MultipleChoice, "Chemistry", "H2O is?", "[\"Water\",\"Salt\"]", "Water"));

        var questions = await GetQuestionsAsync(quiz.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz.Id, [new SubmittedAnswer(questions[0].Id, "Water")]));

        var masteryResponse = await client.GetAsync("/api/v1/quiz/mastery");
        var mastery = await masteryResponse.Content.ReadFromJsonAsync<List<TopicMasteryDto>>();

        mastery.Should().ContainSingle(m => m.Topic == "Chemistry" && m.MasteryScore == 1.0 && m.AttemptsCount == 1);
    }

    [Fact]
    public async Task SubmitQuiz_SecondAttempt_AppliesWeightedMovingAverageNotOverwrite()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        var quiz1 = await SeedQuizAsync(userId, "Physics", Difficulty.Medium, (QuestionType.MultipleChoice, "Physics", "F=ma?", "[\"Yes\",\"No\"]", "Yes"));
        var q1 = await GetQuestionsAsync(quiz1.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz1.Id, [new SubmittedAnswer(q1[0].Id, "Yes")])); // correct -> mastery = 1.0

        var quiz2 = await SeedQuizAsync(userId, "Physics", Difficulty.Medium, (QuestionType.MultipleChoice, "Physics", "E=mc2?", "[\"Yes\",\"No\"]", "Yes"));
        var q2 = await GetQuestionsAsync(quiz2.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz2.Id, [new SubmittedAnswer(q2[0].Id, "No")])); // incorrect -> mastery moves toward 0, not straight to 0

        var mastery = await (await client.GetAsync("/api/v1/quiz/mastery")).Content.ReadFromJsonAsync<List<TopicMasteryDto>>();
        var physics = mastery!.Single(m => m.Topic == "Physics");

        // alpha=0.3: new = 1.0*(1-0.3) + 0*0.3 = 0.7 — averaged, not reset to 0.
        physics.MasteryScore.Should().BeApproximately(0.7, 0.01);
        physics.AttemptsCount.Should().Be(2);
    }

    [Fact]
    public async Task SubmitQuiz_NotOwner_ReturnsNotFound()
    {
        var (ownerClient, ownerId) = await AuthenticatedClientAsync();
        var (otherClient, _) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(ownerId, "Algebra", Difficulty.Easy, (QuestionType.MultipleChoice, "Algebra", "2+2=?", "[\"3\",\"4\"]", "4"));
        _ = ownerClient;

        var questions = await GetQuestionsAsync(quiz.Id);
        var response = await otherClient.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz.Id, [new SubmittedAnswer(questions[0].Id, "4")]));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------------------------------
    // Weak topics / history / attempts
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetWeakTopics_ReturnsOnlyTopicsBelowThreshold()
    {
        var (client, userId) = await AuthenticatedClientAsync();

        var strongQuiz = await SeedQuizAsync(userId, "Strong Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "Strong Topic", "Q?", "[\"A\",\"B\"]", "A"));
        var strongQuestions = await GetQuestionsAsync(strongQuiz.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(strongQuiz.Id, [new SubmittedAnswer(strongQuestions[0].Id, "A")]));

        var weakQuiz = await SeedQuizAsync(userId, "Weak Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "Weak Topic", "Q?", "[\"A\",\"B\"]", "A"));
        var weakQuestions = await GetQuestionsAsync(weakQuiz.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(weakQuiz.Id, [new SubmittedAnswer(weakQuestions[0].Id, "B")]));

        var weakTopics = await (await client.GetAsync("/api/v1/quiz/weak-topics")).Content.ReadFromJsonAsync<List<TopicMasteryDto>>();

        weakTopics.Should().ContainSingle(t => t.Topic == "Weak Topic");
        weakTopics.Should().NotContain(t => t.Topic == "Strong Topic");
    }

    [Fact]
    public async Task GetQuizHistory_ReturnsCompletedAttempts()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "History Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "History Topic", "Q?", "[\"A\",\"B\"]", "A"));
        var questions = await GetQuestionsAsync(quiz.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz.Id, [new SubmittedAnswer(questions[0].Id, "A")]));

        var history = await (await client.GetAsync("/api/v1/quiz/history")).Content.ReadFromJsonAsync<PagedResult<QuizHistoryItemDto>>();

        history!.Items.Should().ContainSingle(h => h.QuizId == quiz.Id && h.Status == "Completed" && h.Score == 100.0);
    }

    [Fact]
    public async Task GetAttempt_ReturnsFullReviewWithExplanations()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "Review Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "Review Topic", "Q?", "[\"A\",\"B\"]", "A"));
        var questions = await GetQuestionsAsync(quiz.Id);
        var submitResult = await (await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz.Id, [new SubmittedAnswer(questions[0].Id, "A")])))
            .Content.ReadFromJsonAsync<QuizAttemptResultDto>();

        var attempt = await (await client.GetAsync($"/api/v1/quiz/attempts/{submitResult!.AttemptId}")).Content.ReadFromJsonAsync<QuizAttemptResultDto>();

        attempt!.Answers.Should().ContainSingle();
        attempt.Answers[0].Explanation.Should().NotBeNullOrWhiteSpace();
        attempt.Answers[0].CorrectAnswer.Should().Be("A");
    }

    // ---------------------------------------------------------------------------------------
    // Retry / delete
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task RetryQuiz_ReturnsSameQuestions()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "Retry Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "Retry Topic", "Q?", "[\"A\",\"B\"]", "A"));

        var response = await client.PostAsync($"/api/v1/quiz/{quiz.Id}/retry", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var retried = await response.Content.ReadFromJsonAsync<QuizDto>();
        retried!.Id.Should().Be(quiz.Id);
        retried.Questions.Should().ContainSingle(q => q.Text == "Q?");
    }

    [Fact]
    public async Task DeleteQuiz_CascadesQuestionsAndAttempts()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "Delete Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "Delete Topic", "Q?", "[\"A\",\"B\"]", "A"));
        var questions = await GetQuestionsAsync(quiz.Id);
        await client.PostAsJsonAsync("/api/v1/quiz/submit", new SubmitQuizRequest(quiz.Id, [new SubmittedAnswer(questions[0].Id, "A")]));

        var deleteResponse = await client.DeleteAsync($"/api/v1/quiz/{quiz.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/quiz/{quiz.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.QuizQuestions.CountAsync(q => q.QuizId == quiz.Id)).Should().Be(0);
        (await db.QuizAttempts.CountAsync(a => a.QuizId == quiz.Id)).Should().Be(0);
    }

    [Fact]
    public async Task DeleteQuiz_AlreadyDeleted_ReturnsNotFound()
    {
        var (client, userId) = await AuthenticatedClientAsync();
        var quiz = await SeedQuizAsync(userId, "Gone Topic", Difficulty.Easy, (QuestionType.MultipleChoice, "Gone Topic", "Q?", "[\"A\",\"B\"]", "A"));
        await client.DeleteAsync($"/api/v1/quiz/{quiz.Id}");

        var response = await client.DeleteAsync($"/api/v1/quiz/{quiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<List<QuestionDto>> GetQuestionsAsync(Guid quizId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var questions = await db.QuizQuestions.Where(q => q.QuizId == quizId).OrderBy(q => q.Order).ToListAsync();
        return questions.Select(QuestionDto.FromDomain).ToList();
    }
}
