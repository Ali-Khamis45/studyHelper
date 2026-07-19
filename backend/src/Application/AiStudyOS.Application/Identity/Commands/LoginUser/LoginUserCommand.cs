using AiStudyOS.Application.Identity.Dtos;
using Mediator;

namespace AiStudyOS.Application.Identity.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password, string? ClientIp) : ICommand<AuthResultDto>;
