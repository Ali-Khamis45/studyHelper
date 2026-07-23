using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetAnalyticsOverview;

public record GetAnalyticsOverviewQuery(DateOnly? From = null, DateOnly? To = null) : IQuery<AnalyticsOverviewDto>;
