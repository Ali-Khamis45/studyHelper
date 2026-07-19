using AiStudyOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AiStudyOS.Api.IntegrationTests;

public class AuthApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("aistudyos_test")
        .WithUsername("aistudyos")
        .WithPassword("test-password")
        .Build();

    // Override point for subclasses (e.g. a rate-limit test factory that needs a much lower
    // threshold than the shared default) to layer extra config on top without duplicating the
    // Postgres/JWT setup below.
    protected virtual IReadOnlyDictionary<string, string?> AdditionalConfiguration => new Dictionary<string, string?>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-bytes-long",
                ["Jwt:Issuer"] = "AiStudyOS.Tests",
                ["Jwt:Audience"] = "AiStudyOS.Tests",
                ["Jwt:AccessTokenLifetimeMinutes"] = "15",
                ["Jwt:RefreshTokenLifetimeDays"] = "21",
            });
            config.AddInMemoryCollection(AdditionalConfiguration);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await DisposeAsync();
    }
}
