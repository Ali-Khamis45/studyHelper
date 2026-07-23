using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Queries.GetTopicMastery;

public record GetTopicMasteryQuery : IQuery<IReadOnlyList<TopicMasteryDto>>;
