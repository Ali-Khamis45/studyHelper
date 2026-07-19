using AiStudyOS.Domain.Common;

namespace AiStudyOS.Domain.Identity;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc <= nowUtc;
    public bool IsActive(DateTime nowUtc) => !IsRevoked && !IsExpired(nowUtc);

    private RefreshToken() { }

    public static RefreshToken IssueNew(Guid userId, string tokenHash, DateTime nowUtc, TimeSpan lifetime, string? createdByIp, Guid? familyId = null) =>
        new()
        {
            UserId = userId,
            FamilyId = familyId ?? Guid.NewGuid(),
            TokenHash = tokenHash,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(lifetime),
            CreatedByIp = createdByIp,
        };

    public void Revoke(DateTime nowUtc, string? replacedByTokenHash = null)
    {
        RevokedAtUtc = nowUtc;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
