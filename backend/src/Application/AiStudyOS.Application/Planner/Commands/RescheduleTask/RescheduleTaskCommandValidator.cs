using AiStudyOS.Application.Common.Interfaces;
using FluentValidation;

namespace AiStudyOS.Application.Planner.Commands.RescheduleTask;

public class RescheduleTaskCommandValidator : AbstractValidator<RescheduleTaskCommand>
{
    public RescheduleTaskCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(dateTimeProvider.UtcNow))
            .WithMessage("New date must be today or later.");
    }
}
