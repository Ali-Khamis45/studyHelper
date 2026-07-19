using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Identity.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiStudyOS.Application.Identity.Commands.Logout;

public class LogoutCommandHandler(IApplicationDbContext db, IDateTimeProvider dateTimeProvider, ILogger<LogoutCommandHandler> logger) : ICommandHandler<LogoutCommand, bool>
{
    public async ValueTask<bool> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenGenerator.Hash(command.RawRefreshToken);

        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (token is null || token.IsRevoked)
            return false;

        token.Revoke(dateTimeProvider.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} logged out", token.UserId);

        return true;
    }
}
