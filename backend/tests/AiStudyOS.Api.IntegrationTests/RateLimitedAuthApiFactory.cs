namespace AiStudyOS.Api.IntegrationTests;

// Deliberately low PermitLimit so RateLimitingTests can trip the "auth" policy in a handful of
// requests instead of needing 20+, and — since xUnit's IClassFixture gives each test class its
// own factory instance — without affecting the rate-limit budget any other test class sees.
public class RateLimitedAuthApiFactory : AuthApiFactory
{
    protected override IReadOnlyDictionary<string, string?> AdditionalConfiguration => new Dictionary<string, string?>
    {
        ["RateLimiting:PermitLimit"] = "3",
        ["RateLimiting:WindowSeconds"] = "60",
    };
}
