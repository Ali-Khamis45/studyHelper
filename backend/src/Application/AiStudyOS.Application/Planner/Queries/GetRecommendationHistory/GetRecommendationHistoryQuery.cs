using AiStudyOS.Application.Planner.Dtos;
using Mediator;

namespace AiStudyOS.Application.Planner.Queries.GetRecommendationHistory;

public record GetRecommendationHistoryQuery : IQuery<IReadOnlyList<PlannerRecommendationDto>>;
