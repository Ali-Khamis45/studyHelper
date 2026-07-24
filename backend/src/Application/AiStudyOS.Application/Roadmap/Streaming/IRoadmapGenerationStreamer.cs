using AiStudyOS.Application.Roadmap.Ai;
using AiStudyOS.Application.Roadmap.Dtos;

namespace AiStudyOS.Application.Roadmap.Streaming;

public abstract record RoadmapGenerationStreamEvent;

public record RoadmapGenerationDeltaEvent(string Content) : RoadmapGenerationStreamEvent;

public record RoadmapGenerationCompleteEvent(RoadmapDto Roadmap) : RoadmapGenerationStreamEvent;

public record RoadmapGenerationErrorEvent(string Message) : RoadmapGenerationStreamEvent;

/// <summary>Drives the same Supervisor -> Agent Registry -> Context Builder -> Prompt Library -> IAiKernel pipeline as GenerateRoadmapCommandHandler, but through IAiKernel.ExecuteStreamAsync — mirrors IQuizGenerationStreamer/IRecommendationStreamer.</summary>
public interface IRoadmapGenerationStreamer
{
    IAsyncEnumerable<RoadmapGenerationStreamEvent> StreamGenerateAsync(RoadmapProfile profile, CancellationToken ct);
}
