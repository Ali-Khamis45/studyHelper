using AiStudyOS.Application.Planner.Dtos;
using Mediator;

namespace AiStudyOS.Application.Planner.Queries.GetToday;

public record GetTodayQuery : IQuery<TodayPlanDto>;
