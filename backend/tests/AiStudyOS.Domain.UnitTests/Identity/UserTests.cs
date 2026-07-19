using AiStudyOS.Domain.Identity;
using FluentAssertions;

namespace AiStudyOS.Domain.UnitTests.Identity;

public class UserTests
{
    [Fact]
    public void Register_creates_unverified_user_with_no_password_hash()
    {
        var user = User.Register("user@example.com", "Name");

        user.Email.Should().Be("user@example.com");
        user.EmailVerified.Should().BeFalse();
        user.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void SetPasswordHash_updates_hash_and_timestamp()
    {
        var user = User.Register("user@example.com", "Name");
        var before = user.UpdatedAtUtc;

        user.SetPasswordHash("hashed-value");

        user.PasswordHash.Should().Be("hashed-value");
        user.UpdatedAtUtc.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void RegisterFromGoogle_creates_verified_user()
    {
        var user = User.RegisterFromGoogle("user@example.com", "Name", "google-id-123", "https://avatar");

        user.EmailVerified.Should().BeTrue();
        user.GoogleId.Should().Be("google-id-123");
    }
}
