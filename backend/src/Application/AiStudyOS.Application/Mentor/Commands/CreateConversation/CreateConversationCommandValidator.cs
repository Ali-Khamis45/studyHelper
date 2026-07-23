using FluentValidation;

namespace AiStudyOS.Application.Mentor.Commands.CreateConversation;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
    }
}
