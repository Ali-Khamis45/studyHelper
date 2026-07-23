using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Queries.GetQuizAnalytics;

public record GetQuizAnalyticsQuery : IQuery<QuizAnalyticsDto>;
