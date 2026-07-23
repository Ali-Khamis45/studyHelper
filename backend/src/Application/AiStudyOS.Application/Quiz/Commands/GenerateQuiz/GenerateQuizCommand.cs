using AiStudyOS.Application.Quiz.Dtos;
using AiStudyOS.Domain.Quiz;
using Mediator;

namespace AiStudyOS.Application.Quiz.Commands.GenerateQuiz;

public record GenerateQuizCommand(
    string? Topic,
    Guid? GoalId,
    Difficulty Difficulty,
    IReadOnlyList<QuestionType> QuestionTypes,
    int QuestionCount,
    QuizType QuizType) : ICommand<QuizDto>;
