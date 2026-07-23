using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Domain.Quiz;

namespace AiStudyOS.Application.Quiz.Streaming;

public abstract record QuizGenerationStreamEvent;

public record QuizGenerationDeltaEvent(string Content) : QuizGenerationStreamEvent;

public record QuizGenerationCompleteEvent(QuizDto Quiz) : QuizGenerationStreamEvent;

public record QuizGenerationErrorEvent(string Message) : QuizGenerationStreamEvent;

/// <summary>Drives the same Supervisor -> Agent Registry -> Context Builder -> Prompt Library -> IAiKernel pipeline as GenerateQuizCommandHandler, but through IAiKernel.ExecuteStreamAsync — mirrors IRecommendationStreamer/IMentorMessageStreamer.</summary>
public interface IQuizGenerationStreamer
{
    IAsyncEnumerable<QuizGenerationStreamEvent> StreamGenerateAsync(
        string? topic, Guid? goalId, Difficulty difficulty, IReadOnlyList<QuestionType> questionTypes, int questionCount, QuizType quizType, CancellationToken ct);
}
