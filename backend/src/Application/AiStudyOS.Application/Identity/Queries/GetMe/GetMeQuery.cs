using AiStudyOS.Application.Identity.Dtos;
using Mediator;

namespace AiStudyOS.Application.Identity.Queries.GetMe;

public record GetMeQuery : IQuery<UserDto>;
