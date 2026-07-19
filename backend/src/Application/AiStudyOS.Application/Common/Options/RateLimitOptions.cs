namespace AiStudyOS.Application.Common.Options;

public class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; init; } = 20;
    public int WindowSeconds { get; init; } = 60;
}
