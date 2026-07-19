using System.Net;
using System.Net.Http.Json;
using AiStudyOS.Api.Endpoints;
using FluentAssertions;

namespace AiStudyOS.Api.IntegrationTests;

public class RateLimitingTests(RateLimitedAuthApiFactory factory) : IClassFixture<RateLimitedAuthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Exceeding_the_configured_limit_returns_429()
    {
        // Factory is configured with PermitLimit=3 — the first 3 requests should not be
        // rate-limited (whatever their own outcome), the 4th must be.
        for (var i = 0; i < 3; i++)
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/auth/register",
                new RegisterRequest($"rate-limit-{Guid.NewGuid():N}@example.com", "a-valid-password1", "Rate Limit Test"));

            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var throttled = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest($"rate-limit-{Guid.NewGuid():N}@example.com", "a-valid-password1", "Rate Limit Test"));

        throttled.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
