using AiStudyOS.Application.Common.Exceptions;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Domain.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AiStudyOS.Application.Identity.Queries.GetMe;

public class GetMeQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IQueryHandler<GetMeQuery, UserDto>
{
    public async ValueTask<UserDto> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidCredentialsException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        return UserDto.FromDomain(user);
    }
}
