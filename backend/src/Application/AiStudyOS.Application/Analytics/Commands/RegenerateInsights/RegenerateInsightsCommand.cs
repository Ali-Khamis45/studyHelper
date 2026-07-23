using AiStudyOS.Application.Analytics.Dtos;
using Mediator;

namespace AiStudyOS.Application.Analytics.Commands.RegenerateInsights;

public record RegenerateInsightsCommand : ICommand<InsightsDto>;
