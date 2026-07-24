using AiStudyOS.Application.Roadmap.Dtos;
using Mediator;

namespace AiStudyOS.Application.Roadmap.Queries.GetRoadmaps;

public record GetRoadmapsQuery : IQuery<IReadOnlyList<RoadmapSummaryDto>>;
