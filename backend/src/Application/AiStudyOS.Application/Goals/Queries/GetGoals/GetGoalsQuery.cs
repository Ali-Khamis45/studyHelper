using AiStudyOS.Application.Goals.Dtos;
using AiStudyOS.Domain.Goals;
using Mediator;

namespace AiStudyOS.Application.Goals.Queries.GetGoals;

public record GetGoalsQuery(GoalStatus? Status = null) : IQuery<IReadOnlyList<GoalDto>>;
