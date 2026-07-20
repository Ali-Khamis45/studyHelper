using AiStudyOS.Application.Planner.Dtos;
using Mediator;

namespace AiStudyOS.Application.Planner.Commands.GenerateDailyRecommendation;

public record GenerateDailyRecommendationCommand : ICommand<TodayPlanDto>;
