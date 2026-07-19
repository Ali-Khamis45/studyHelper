namespace AiStudyOS.Application.Common.Options;

// Named AccountLockoutOptions (not LockoutOptions) to avoid colliding with
// Microsoft.AspNetCore.Identity.LockoutOptions, which is in scope wherever IPasswordHasher<User>
// is used.
public class AccountLockoutOptions
{
    public const string SectionName = "Lockout";

    public int MaxFailedAttempts { get; init; } = 5;
    public int LockoutDurationMinutes { get; init; } = 15;
}
