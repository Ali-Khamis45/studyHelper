using AiStudyOS.Application.Common.Options;
using AiStudyOS.Domain.Quiz;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Quiz.Commands.GenerateQuiz;

public class GenerateQuizCommandValidator : AbstractValidator<GenerateQuizCommand>
{
    public GenerateQuizCommandValidator(IOptions<QuizOptions> options)
    {
        var quizOptions = options.Value;

        RuleFor(x => x.Topic).NotEmpty().MaximumLength(200).Unless(x => x.QuizType == QuizType.Review);
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.QuizType).IsInEnum();
        RuleFor(x => x.QuestionTypes).NotEmpty().WithMessage("At least one question type must be allowed.");
        RuleForEach(x => x.QuestionTypes).IsInEnum();
        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(quizOptions.MinQuestionCount, quizOptions.MaxQuestionCount);
    }
}
