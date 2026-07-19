namespace AiStudyOS.Application.Common.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshTokenLifetimeDays { get; init; } = 21;
}
