using AiStudyOS.Domain.Identity;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Identity;

public class RefreshTokenTests
{
    [Fact]
    public void IssueNew_without_explicit_family_generates_new_family_id()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.IssueNew(Guid.NewGuid(), "hash", now, TimeSpan.FromDays(1), "127.0.0.1");

        token.FamilyId.Should().NotBeEmpty();
        token.IsActive(now).Should().BeTrue();
    }

    [Fact]
    public void IssueNew_with_explicit_family_preserves_it_across_rotation()
    {
        var now = DateTime.UtcNow;
        var original = RefreshToken.IssueNew(Guid.NewGuid(), "hash1", now, TimeSpan.FromDays(1), null);
        var rotated = RefreshToken.IssueNew(original.UserId, "hash2", now, TimeSpan.FromDays(1), null, original.FamilyId);

        rotated.FamilyId.Should().Be(original.FamilyId);
    }

    [Fact]
    public void Revoke_marks_token_inactive()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.IssueNew(Guid.NewGuid(), "hash", now, TimeSpan.FromDays(1), null);

        token.Revoke(now.AddMinutes(1), "replacement-hash");

        token.IsRevoked.Should().BeTrue();
        token.IsActive(now.AddMinutes(2)).Should().BeFalse();
        token.ReplacedByTokenHash.Should().Be("replacement-hash");
    }

    [Fact]
    public void IsExpired_true_once_past_expiry()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.IssueNew(Guid.NewGuid(), "hash", now, TimeSpan.FromMinutes(1), null);

        token.IsExpired(now.AddMinutes(2)).Should().BeTrue();
        token.IsActive(now.AddMinutes(2)).Should().BeFalse();
    }
}
