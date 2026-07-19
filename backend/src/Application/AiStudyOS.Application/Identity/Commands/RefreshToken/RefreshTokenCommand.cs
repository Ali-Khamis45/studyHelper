using AiStudyOS.Application.Identity.Dtos;
using Mediator;

namespace AiStudyOS.Application.Identity.Commands.RefreshToken;

public record RefreshTokenCommand(string RawRefreshToken, string? ClientIp) : ICommand<AuthResultDto>;
