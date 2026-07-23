using AiStudyOS.Application.Quiz.Dtos;
using Mediator;

namespace AiStudyOS.Application.Quiz.Commands.SubmitQuiz;

public record SubmittedAnswer(Guid QuestionId, string Answer);

public record SubmitQuizCommand(Guid QuizId, IReadOnlyList<SubmittedAnswer> Answers) : ICommand<QuizAttemptResultDto>;
