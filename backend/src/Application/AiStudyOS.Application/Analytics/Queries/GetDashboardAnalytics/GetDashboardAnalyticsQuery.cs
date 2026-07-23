using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetDashboardAnalytics;

public record GetDashboardAnalyticsQuery : IQuery<DashboardAnalyticsDto>;
