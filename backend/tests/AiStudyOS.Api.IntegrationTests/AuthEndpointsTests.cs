using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiStudyOS.Api.Endpoints;
using AiStudyOS.Application.Identity.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AiStudyOS.Api.IntegrationTests;

public class AuthEndpointsTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // HandleCookies=false: tests manage the refresh-token cookie explicitly (including reusing
    // stale/revoked values on purpose), which the client's own automatic CookieContainer would
    // otherwise silently override with whatever cookie it last saw.
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private async Task<(AuthResponse Body, string RefreshCookie)> RegisterAsync(string? email = null, string password = "correct-horse-battery")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email ?? UniqueEmail(), password, "Test User"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return (body!, ExtractRefreshCookie(response));
    }

    private static string ExtractRefreshCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.GetValues("Set-Cookie").First(c => c.StartsWith("refreshToken="));
        return setCookie.Split(';')[0];
    }

    private static HttpRequestMessage WithCookie(HttpMethod method, string url, string cookie)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", cookie);
        return request;
    }

    [Fact]
    public async Task Register_ReturnsCreated_WithAccessTokenAndRefreshCookie()
    {
        var (body, cookie) = await RegisterAsync();

        body.AccessToken.Should().NotBeNullOrEmpty();
        body.User.Email.Should().NotBeNullOrEmpty();
        cookie.Should().StartWith("refreshToken=");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "another-password1", "Test User 2"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(UniqueEmail(), "short", "Test User"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = UniqueEmail();
        await RegisterAsync(email, "correct-password1");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "wrong-password1"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsOkAndSetsCookie()
    {
        var email = UniqueEmail();
        await RegisterAsync(email, "correct-password1");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "correct-password1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ExtractRefreshCookie(response).Should().StartWith("refreshToken=");
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsMatchingUser()
    {
        var (body, _) = await RegisterAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", body.AccessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be(body.User.Email);
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndOldCookieBecomesInvalid()
    {
        var (_, oldCookie) = await RegisterAsync();

        var refreshResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/refresh", oldCookie));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newCookie = ExtractRefreshCookie(refreshResponse);
        newCookie.Should().NotBe(oldCookie);

        var reuseResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/refresh", oldCookie));
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ReuseOfRevokedToken_RevokesEntireFamily()
    {
        var (_, firstCookie) = await RegisterAsync();

        var secondResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/refresh", firstCookie));
        var secondCookie = ExtractRefreshCookie(secondResponse);

        // Reusing the already-rotated first cookie must be detected and kill the whole family.
        var reuseResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/refresh", firstCookie));
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // The previously-valid second cookie must now also be dead.
        var thirdResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/refresh", secondCookie));
        thirdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesToken_SubsequentRefreshFails()
    {
        var (_, cookie) = await RegisterAsync();

        var logoutResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/logout", cookie));
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await _client.SendAsync(WithCookie(HttpMethod.Post, "/api/v1/auth/refresh", cookie));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
