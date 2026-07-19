using AiStudyOS.Application.Identity.Commands.LoginUser;
using AiStudyOS.Application.Identity.Commands.Logout;
using AiStudyOS.Application.Identity.Commands.RefreshToken;
using AiStudyOS.Application.Identity.Commands.RegisterUser;
using AiStudyOS.Application.Identity.Dtos;
using AiStudyOS.Application.Identity.Queries.GetMe;
using Mediator;

namespace AiStudyOS.Api.Endpoints;

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, UserDto User);

public static class AuthEndpoints
{
    private const string RefreshCookieName = "refreshToken";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IMediator mediator, HttpContext http, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RegisterUserCommand(request.Email, request.Password, request.DisplayName, GetClientIp(http)), ct);
            SetRefreshCookie(http, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
            return Results.Created("/api/v1/auth/me", ToResponse(result));
        }).AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IMediator mediator, HttpContext http, CancellationToken ct) =>
        {
            var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password, GetClientIp(http)), ct);
            SetRefreshCookie(http, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
            return Results.Ok(ToResponse(result));
        }).AllowAnonymous();

        group.MapPost("/refresh", async (IMediator mediator, HttpContext http, CancellationToken ct) =>
        {
            var rawToken = http.Request.Cookies[RefreshCookieName];
            if (string.IsNullOrEmpty(rawToken))
                return Results.Unauthorized();

            var result = await mediator.Send(new RefreshTokenCommand(rawToken, GetClientIp(http)), ct);
            SetRefreshCookie(http, result.RefreshToken, result.RefreshTokenExpiresAtUtc);
            return Results.Ok(ToResponse(result));
        }).AllowAnonymous();

        group.MapPost("/logout", async (IMediator mediator, HttpContext http, CancellationToken ct) =>
        {
            var rawToken = http.Request.Cookies[RefreshCookieName];
            if (!string.IsNullOrEmpty(rawToken))
                await mediator.Send(new LogoutCommand(rawToken), ct);

            ClearRefreshCookie(http);
            return Results.NoContent();
        }).AllowAnonymous();

        group.MapGet("/me", async (IMediator mediator, CancellationToken ct) =>
        {
            var user = await mediator.Send(new GetMeQuery(), ct);
            return Results.Ok(user);
        });

        return app;
    }

    private static AuthResponse ToResponse(AuthResultDto result) => new(result.AccessToken, result.AccessTokenExpiresAtUtc, result.User);

    private static string? GetClientIp(HttpContext http) => http.Connection.RemoteIpAddress?.ToString();

    private static void SetRefreshCookie(HttpContext http, string rawToken, DateTime expiresAtUtc)
    {
        http.Response.Cookies.Append(RefreshCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = http.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            // Path="/" rather than scoped to /api/v1/auth: the frontend reaches these endpoints
            // through a same-origin Next.js rewrite (/api/backend/*), so from the browser's
            // perspective the Set-Cookie response comes from that rewritten path, not the
            // backend's own route — a narrower Path here would never match on the way back.
            Path = "/",
            Expires = expiresAtUtc,
        });
    }

    private static void ClearRefreshCookie(HttpContext http) =>
        http.Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/" });
}
