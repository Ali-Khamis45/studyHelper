using AiStudyOS.Application.Common.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Mentor.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator(IOptions<MentorOptions> options)
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(options.Value.MessageMaxLength);
    }
}
