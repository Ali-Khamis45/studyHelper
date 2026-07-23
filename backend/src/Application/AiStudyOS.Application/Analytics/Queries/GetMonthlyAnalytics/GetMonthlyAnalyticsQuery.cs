using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetMonthlyAnalytics;

public record GetMonthlyAnalyticsQuery : IQuery<PeriodAnalyticsDto>;
