using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Identity.Services;
using AiStudyOS.Domain.Identity;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Identity.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtOptions> jwtOptions) : ICommandHandler<RegisterUserCommand, AuthResultDto>
{
    public async ValueTask<AuthResultDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
            throw new EmailAlreadyExistsException(normalizedEmail);

        var user = User.Register(normalizedEmail, command.DisplayName.Trim());
        user.SetPasswordHash(passwordHasher.HashPassword(user, command.Password));

        var now = dateTimeProvider.UtcNow;
        var rawRefreshToken = RefreshTokenGenerator.GenerateRawToken();
        var refreshToken = AiStudyOS.Domain.Identity.RefreshToken.IssueNew(
            user.Id,
            RefreshTokenGenerator.Hash(rawRefreshToken),
            now,
            TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays),
            command.ClientIp);

        db.Users.Add(user);
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAtUtc, rawRefreshToken, refreshToken.ExpiresAtUtc, UserDto.FromDomain(user));
    }
}
