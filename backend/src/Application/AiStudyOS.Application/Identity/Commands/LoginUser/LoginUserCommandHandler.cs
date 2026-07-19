using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Identity.Services;
using AiStudyOS.Domain.Identity;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Identity.Commands.LoginUser;

public class LoginUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AccountLockoutOptions> lockoutOptions,
    ILogger<LoginUserCommandHandler> logger) : ICommandHandler<LoginUserCommand, AuthResultDto>
{
    public async ValueTask<AuthResultDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var now = dateTimeProvider.UtcNow;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Same generic failure for "no such user", "locked out", and "wrong password" — lockout
        // state must not become an oracle an attacker can use to confirm an account exists or
        // is close to locking.
        if (user is null || user.PasswordHash is null)
        {
            logger.LogWarning("Login failed for unknown email {Email} from {ClientIp}", normalizedEmail, command.ClientIp);
            throw new InvalidCredentialsException();
        }

        if (user.IsLockedOut(now))
        {
            logger.LogWarning("Login rejected for locked-out account {UserId} from {ClientIp}", user.Id, command.ClientIp);
            throw new InvalidCredentialsException();
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            user.RegisterFailedLogin(now, lockoutOptions.Value.MaxFailedAttempts, TimeSpan.FromMinutes(lockoutOptions.Value.LockoutDurationMinutes));
            await db.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                "Login failed for account {UserId} from {ClientIp} (attempt {Attempt}/{MaxAttempts}){LockoutSuffix}",
                user.Id, command.ClientIp, user.FailedLoginAttempts, lockoutOptions.Value.MaxFailedAttempts,
                user.IsLockedOut(now) ? " — account now locked" : "");

            throw new InvalidCredentialsException();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.SetPasswordHash(passwordHasher.HashPassword(user, command.Password));

        user.RegisterSuccessfulLogin(now);

        var rawRefreshToken = RefreshTokenGenerator.GenerateRawToken();
        var refreshToken = AiStudyOS.Domain.Identity.RefreshToken.IssueNew(
            user.Id,
            RefreshTokenGenerator.Hash(rawRefreshToken),
            now,
            TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays),
            command.ClientIp);

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Login succeeded for account {UserId} from {ClientIp}", user.Id, command.ClientIp);

        var accessToken = jwtTokenService.GenerateAccessToken(user);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAtUtc, rawRefreshToken, refreshToken.ExpiresAtUtc, UserDto.FromDomain(user));
    }
}
