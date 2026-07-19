using AiStudyOS.Application.Common.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Identity.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(IOptions<PasswordPolicyOptions> passwordPolicy)
    {
        var policy = passwordPolicy.Value;

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(policy.MinimumLength).MaximumLength(policy.MaximumLength);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
    }
}
