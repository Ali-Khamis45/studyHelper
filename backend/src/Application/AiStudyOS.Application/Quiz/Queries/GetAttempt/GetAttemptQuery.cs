using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Queries.GetAttempt;

public record GetAttemptQuery(Guid AttemptId) : IQuery<QuizAttemptResultDto>;
