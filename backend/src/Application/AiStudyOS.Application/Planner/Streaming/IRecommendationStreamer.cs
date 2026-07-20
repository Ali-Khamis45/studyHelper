using AiStudyOS.Application.Planner.Dtos;

namespace AiStudyOS.Application.Planner.Streaming;

public abstract record RecommendationStreamEvent;

public record RecommendationDeltaEvent(string Content) : RecommendationStreamEvent;

public record RecommendationCompleteEvent(TodayPlanDto Plan) : RecommendationStreamEvent;

public record RecommendationErrorEvent(string Message) : RecommendationStreamEvent;

/// <summary>
/// Drives the same AI Kernel -> Provider pipeline as GenerateDailyRecommendationCommandHandler, but
/// through IAiKernel.ExecuteStreamAsync so callers can render incremental output instead of waiting
/// for the full response (§4). Converges on RecommendationFinalizer for persistence, so streamed and
/// non-streamed generation never diverge in what gets written to the database.
/// </summary>
public interface IRecommendationStreamer
{
    IAsyncEnumerable<RecommendationStreamEvent> StreamTodayRecommendationAsync(CancellationToken ct);
}
