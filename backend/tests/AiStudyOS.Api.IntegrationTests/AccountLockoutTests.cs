using System.Net;
using System.Net.Http.Json;
using AiStudyOS.Api.Endpoints;
using FluentAssertions;

namespace AiStudyOS.Api.IntegrationTests;

public class AccountLockoutTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // Matches appsettings.json's default Lockout:MaxFailedAttempts.
    private const int MaxFailedAttempts = 5;

    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueEmail() => $"lockout-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task After_max_failed_attempts_correct_password_still_rejected()
    {
        var email = UniqueEmail();
        const string correctPassword = "correct-horse-battery";

        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, correctPassword, "Lockout Test"));
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        for (var attempt = 0; attempt < MaxFailedAttempts; attempt++)
        {
            var failedAttempt = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "wrong-password"));
            failedAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Account is now locked — even the correct password must be rejected, with the same
        // generic message as a wrong password (verified by status code; message is identical
        // by construction in LoginUserCommandHandler).
        var lockedOutAttempt = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, correctPassword));
        lockedOutAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Successful_login_resets_failed_attempt_counter()
    {
        var email = UniqueEmail();
        const string correctPassword = "correct-horse-battery";

        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, correctPassword, "Lockout Reset Test"));

        // A couple of failures, but fewer than the lockout threshold.
        await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "wrong-password"));
        await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "wrong-password"));

        var successfulLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, correctPassword));
        successfulLogin.StatusCode.Should().Be(HttpStatusCode.OK);

        // Counter reset by the successful login — should take a full new run of failures to lock.
        for (var attempt = 0; attempt < MaxFailedAttempts - 1; attempt++)
        {
            var failedAttempt = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "wrong-password"));
            failedAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var stillUnlockedAttempt = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, correctPassword));
        stillUnlockedAttempt.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
