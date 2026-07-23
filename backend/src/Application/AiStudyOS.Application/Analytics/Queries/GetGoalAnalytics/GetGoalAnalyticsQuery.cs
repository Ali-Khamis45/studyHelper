using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetGoalAnalytics;

public record GetGoalAnalyticsQuery : IQuery<GoalAnalyticsDto>;
