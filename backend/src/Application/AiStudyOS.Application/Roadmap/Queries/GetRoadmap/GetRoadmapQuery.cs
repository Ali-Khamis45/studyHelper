using AiStudyOS.Application.Roadmap.Dtos;
using Mediator;

namespace AiStudyOS.Application.Roadmap.Queries.GetRoadmap;

public record GetRoadmapQuery(Guid RoadmapId) : IQuery<RoadmapDto>;
