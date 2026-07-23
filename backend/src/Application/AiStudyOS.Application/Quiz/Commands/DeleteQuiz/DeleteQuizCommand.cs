using Mediator;

namespace AiStudyOS.Application.Quiz.Commands.DeleteQuiz;

public record DeleteQuizCommand(Guid QuizId) : ICommand<bool>;
