using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Queries.GetQuiz;

public record GetQuizQuery(Guid QuizId) : IQuery<QuizDto>;
