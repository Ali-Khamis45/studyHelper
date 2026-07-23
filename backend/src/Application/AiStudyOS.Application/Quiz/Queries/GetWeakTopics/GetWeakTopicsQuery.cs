using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Queries.GetWeakTopics;

public record GetWeakTopicsQuery(int? Take = null) : IQuery<IReadOnlyList<TopicMasteryDto>>;
