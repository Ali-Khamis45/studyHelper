using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetStreakAnalytics;

public record GetStreakAnalyticsQuery : IQuery<StreakAnalyticsDto>;
