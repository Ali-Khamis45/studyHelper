using Mediator;

namespace AiStudyOS.Application.Identity.Commands.Logout;

public record LogoutCommand(string RawRefreshToken) : ICommand<bool>;
