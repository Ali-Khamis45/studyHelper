using AiStudyOS.Domain.Identity;

namespace AiStudyOS.Application.Identity.Dtos;

public record UserDto(Guid Id, string Email, string DisplayName, string? AvatarUrl, string TimeZone)
{
    public static UserDto FromDomain(User user) =>
        new(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.TimeZone);
}
