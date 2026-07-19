using AiStudyOS.Application.Identity.Commands.RegisterUser;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AiStudyOS.Application.UnitTests.Identity;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData("missing-domain@")]
    public void Invalid_email_fails(string email)
    {
        var result = _validator.TestValidate(new RegisterUserCommand(email, "a-valid-password1", "Name", null));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("")]
    public void Too_short_password_fails(string password)
    {
        var result = _validator.TestValidate(new RegisterUserCommand("user@example.com", password, "Name", null));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Empty_display_name_fails()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("user@example.com", "a-valid-password1", "", null));

        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("user@example.com", "a-valid-password1", "Name", "127.0.0.1"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
