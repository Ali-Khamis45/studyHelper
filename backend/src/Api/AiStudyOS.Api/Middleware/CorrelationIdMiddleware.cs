using AiStudyOS.Application.Common.Interfaces;

namespace AiStudyOS.Api.Middleware;

public static class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    /// <summary>
    /// Surfaces the request's ICorrelationIdProvider value (the same one AiKernel, telemetry, and
    /// logs use) as a response header, so a caller can match their own logs against ours.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var correlationId = context.RequestServices.GetRequiredService<ICorrelationIdProvider>().CorrelationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            await next();
        });
}
