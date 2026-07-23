using FluentValidation;

namespace AiStudyOS.Application.Mentor.Commands.RenameConversation;

public class RenameConversationCommandValidator : AbstractValidator<RenameConversationCommand>
{
    public RenameConversationCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
