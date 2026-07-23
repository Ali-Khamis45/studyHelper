using AiStudyOS.Api.Endpoints;
using AiStudyOS.Api.Middleware;
using AiStudyOS.Api.OpenApi;
using AiStudyOS.Application;
using AiStudyOS.Application.Common.Behaviors;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// JWT options are resolved lazily via IOptions<JwtOptions> (bound in AddInfrastructure below)
// rather than read from builder.Configuration here, so WebApplicationFactory-based integration
// tests can override configuration before the host is built.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearerOptions.MapInboundClaims = false;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// No CORS policy is registered — intentionally. The frontend only ever reaches this API through
// a same-origin Next.js rewrite (see frontend/next.config.ts), so no browser cross-origin request
// needs to be allowed; the default (deny all cross-origin) is the correct least-privilege posture.
// If a future client needs true cross-origin access (a mobile app, a separate origin), add a
// narrowly-scoped AddCors policy naming exact allowed origins then — not a wildcard.

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partitioned per-request rather than configured once at startup, so the rate limit itself
    // is resolved from IOptions<RateLimitOptions> lazily (via RequestServices) — same reason the
    // JWT options above are lazy: WebApplicationFactory-based tests override configuration before
    // the host is built, and this keeps that override effective here too.
    options.AddPolicy("auth", httpContext =>
    {
        var rateLimit = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimit.PermitLimit,
            Window = TimeSpan.FromSeconds(rateLimit.WindowSeconds),
            QueueLimit = 0,
        });
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Enums over the wire as their names ("Certification", "High"), not raw ints — applies to both
// Minimal API request/response bodies and anything serialized through ASP.NET Core's JSON options.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSecurityHeaders();
app.UseCorrelationId();

// HSTS is meaningless (and wrong) to send over local http dev — only applies once behind real TLS.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapGoalsEndpoints();
app.MapPlannerEndpoints();
app.MapMentorEndpoints();
app.MapQuizEndpoints();
app.MapAnalyticsEndpoints();
app.MapSystemEndpoints();

app.Run();

public partial class Program;
