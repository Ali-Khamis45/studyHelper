using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Identity;

public class User : AggregateRoot
{
    public string Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string DisplayName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public string? GoogleId { get; private set; }
    public bool EmailVerified { get; private set; }
    public string TimeZone { get; private set; } = "UTC";
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedOutUntilUtc { get; private set; }

    private User() { }

    public static User Register(string email, string displayName, string timeZone = "UTC")
    {
        var now = DateTime.UtcNow;
        return new User
        {
            Email = email,
            DisplayName = displayName,
            TimeZone = timeZone,
            EmailVerified = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static User RegisterFromGoogle(string email, string displayName, string googleId, string? avatarUrl)
    {
        var now = DateTime.UtcNow;
        return new User
        {
            Email = email,
            DisplayName = displayName,
            GoogleId = googleId,
            AvatarUrl = avatarUrl,
            EmailVerified = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    public void LinkGoogleAccount(string googleId, string? avatarUrl)
    {
        GoogleId = googleId;
        AvatarUrl ??= avatarUrl;
        EmailVerified = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsLockedOut(DateTime nowUtc) => LockedOutUntilUtc is { } until && until > nowUtc;

    public void RegisterFailedLogin(DateTime nowUtc, int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
            LockedOutUntilUtc = nowUtc.Add(lockoutDuration);
    }

    public void RegisterSuccessfulLogin(DateTime nowUtc)
    {
        FailedLoginAttempts = 0;
        LockedOutUntilUtc = null;
        UpdatedAtUtc = nowUtc;
    }
}
