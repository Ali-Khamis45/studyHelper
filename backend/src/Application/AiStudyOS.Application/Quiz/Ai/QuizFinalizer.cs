using System.Text.Json;
using AiStudyOS.Application.AI.Telemetry;
using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Ai;

/// <summary>Turns a parsed QuizGenerationResult into a persisted Quiz + QuizQuestions and returns the resulting QuizDto — the one place either the sync or streaming generation path is allowed to persist, mirroring RecommendationFinalizer.</summary>
public static class QuizFinalizer
{
    public static async Task<QuizDto> FinalizeAsync(
        IApplicationDbContext db,
        Guid userId,
        Guid? goalId,
        QuizType quizType,
        QuizGenerationResult data,
        AiTelemetryRecord telemetry,
        DateTime nowUtc,
        CancellationToken ct)
    {
        if (data.Questions.Count == 0)
            throw new AiGenerationFailedException("The AI returned a quiz with no questions.");

        var topic = RequireField(data.Questions[0].Topic, "questions[0].topic");

        // Every field the schema requires is validated before anything is persisted — a JSON object
        // can deserialize successfully (System.Text.Json doesn't enforce non-null on plain `string`
        // properties at runtime) while still being missing content this table's NOT NULL columns
        // require. Title alone gets a real fallback (cosmetic, not a content-integrity problem);
        // every other missing field is treated as a genuine generation failure — worth retrying,
        // not worth silently papering over with placeholder text.
        var title = string.IsNullOrWhiteSpace(data.Title) ? $"{topic} Quiz" : data.Title;

        var quiz = Domain.Quiz.Quiz.Create(
            userId, goalId, title, topic,
            ParseDifficulty(data.Questions[0].Difficulty), quizType, data.Questions.Count,
            telemetry.Model, telemetry.PromptVersion ?? "v1", telemetry.CorrelationId, nowUtc);

        db.Quizzes.Add(quiz);

        var questions = new List<QuizQuestion>();
        var order = 0;
        foreach (var generated in data.Questions)
        {
            var question = QuizQuestion.Create(
                quiz.Id,
                order++,
                ParseQuestionType(generated.Type),
                RequireField(generated.Topic, "topic"),
                ParseDifficulty(generated.Difficulty),
                RequireField(generated.Text, "text"),
                generated.Options is { Count: > 0 } options ? JsonSerializer.Serialize(options) : null,
                RequireField(generated.CorrectAnswer, "correctAnswer"),
                RequireField(generated.Explanation, "explanation"));

            questions.Add(question);
            db.QuizQuestions.Add(question);
        }

        await db.SaveChangesAsync(ct);

        return QuizDto.FromDomain(quiz, questions);
    }

    private static string RequireField(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value) ? throw new AiGenerationFailedException($"The AI response was missing a required '{fieldName}' value.") : value;

    private static QuestionType ParseQuestionType(string raw) =>
        Enum.TryParse<QuestionType>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : throw new AiGenerationFailedException($"Unrecognized question type '{raw}' in AI response.");

    private static Difficulty ParseDifficulty(string raw) =>
        Enum.TryParse<Difficulty>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : throw new AiGenerationFailedException($"Unrecognized difficulty '{raw}' in AI response.");
}
