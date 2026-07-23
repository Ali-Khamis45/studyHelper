using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Commands.RetryQuiz;

public record RetryQuizCommand(Guid QuizId) : ICommand<QuizDto>;
