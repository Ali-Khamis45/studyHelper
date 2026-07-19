using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Identity.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiStudyOS.Application.Identity.Commands.RefreshToken;

// Fully-qualifies AiStudyOS.Domain.Identity.RefreshToken throughout — its simple name collides
// with this feature folder's namespace segment (AiStudyOS.Application.Identity.Commands.RefreshToken).
public class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtOptions> jwtOptions) : ICommandHandler<RefreshTokenCommand, AuthResultDto>
{
    public async ValueTask<AuthResultDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenGenerator.Hash(command.RawRefreshToken);
        var now = dateTimeProvider.UtcNow;

        var existingToken = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException("not found");

        if (existingToken.IsRevoked)
        {
            var family = await db.RefreshTokens
                .Where(t => t.FamilyId == existingToken.FamilyId && t.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);
            foreach (var token in family)
                token.Revoke(now);
            await db.SaveChangesAsync(cancellationToken);

            throw new InvalidRefreshTokenException("reuse detected — token family revoked");
        }

        if (existingToken.IsExpired(now))
            throw new InvalidRefreshTokenException("expired");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken)
            ?? throw new InvalidRefreshTokenException("user not found");

        var rawNewRefreshToken = RefreshTokenGenerator.GenerateRawToken();
        var newHash = RefreshTokenGenerator.Hash(rawNewRefreshToken);

        existingToken.Revoke(now, newHash);

        var newRefreshToken = AiStudyOS.Domain.Identity.RefreshToken.IssueNew(
            user.Id,
            newHash,
            now,
            TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays),
            command.ClientIp,
            existingToken.FamilyId);

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAtUtc, rawNewRefreshToken, newRefreshToken.ExpiresAtUtc, UserDto.FromDomain(user));
    }
}
