using AiStudyOS.Application.Identity.Dtos;
using Mediator;

namespace AiStudyOS.Application.Identity.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string DisplayName, string? ClientIp) : ICommand<AuthResultDto>;
