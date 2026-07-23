namespace AiStudyOS.Api.IntegrationTests;

// MentorEndpointsTests registers a fresh user per test (20+ tests in the class) purely to get an
// isolated, authenticated client — that's normal register volume for this class, not the kind of
// abuse the "auth" rate-limit policy exists to catch, so it needs a much higher budget than the
// production default to avoid tripping on its own setup. Each test class gets its own factory
// instance (IClassFixture), so this doesn't loosen the limit seen by RateLimitingTests or any other class.
public class GenerousRateLimitAuthApiFactory : AuthApiFactory
{
    protected override IReadOnlyDictionary<string, string?> AdditionalConfiguration => new Dictionary<string, string?>
    {
        ["RateLimiting:PermitLimit"] = "200",
        ["RateLimiting:WindowSeconds"] = "60",
    };
}
