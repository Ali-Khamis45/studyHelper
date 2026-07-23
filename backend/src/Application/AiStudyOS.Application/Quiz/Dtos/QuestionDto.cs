using System.Text.Json;
using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Dtos;

/// <summary>
/// The shape returned while a quiz is being taken — deliberately excludes CorrectAnswer and
/// Explanation so the client can never read the answer key before submitting. See AnswerResultDto
/// for the post-grading shape that does include them.
/// </summary>
public record QuestionDto(Guid Id, int Order, string Type, string Topic, string Difficulty, string Text, IReadOnlyList<string>? Options)
{
    public static QuestionDto FromDomain(QuizQuestion question) => new(
        question.Id,
        question.Order,
        question.Type.ToString(),
        question.Topic,
        question.Difficulty.ToString(),
        question.Text,
        question.OptionsJson is null ? null : JsonSerializer.Deserialize<List<string>>(question.OptionsJson));
}
