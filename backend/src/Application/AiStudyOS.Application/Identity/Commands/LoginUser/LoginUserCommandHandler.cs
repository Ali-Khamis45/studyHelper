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

namespace AiStudyOS.Application.Identity.Commands.LoginUser;

public class LoginUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtOptions> jwtOptions) : ICommandHandler<LoginUserCommand, AuthResultDto>
{
    public async ValueTask<AuthResultDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null || user.PasswordHash is null)
            throw new InvalidCredentialsException();

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (verification == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.SetPasswordHash(passwordHasher.HashPassword(user, command.Password));

        var now = dateTimeProvider.UtcNow;
        var rawRefreshToken = RefreshTokenGenerator.GenerateRawToken();
        var refreshToken = AiStudyOS.Domain.Identity.RefreshToken.IssueNew(
            user.Id,
            RefreshTokenGenerator.Hash(rawRefreshToken),
            now,
            TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays),
            command.ClientIp);

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAtUtc, rawRefreshToken, refreshToken.ExpiresAtUtc, UserDto.FromDomain(user));
    }
}
