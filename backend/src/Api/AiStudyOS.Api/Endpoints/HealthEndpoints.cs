namespace AiStudyOS.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/health", () => Results.Ok(new { status = "healthy", timestampUtc = DateTime.UtcNow }))
            .WithName("GetHealth")
            .WithTags("System")
            .AllowAnonymous();

        return app;
    }
}
