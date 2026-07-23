using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetWeeklyAnalytics;

public record GetWeeklyAnalyticsQuery : IQuery<PeriodAnalyticsDto>;
