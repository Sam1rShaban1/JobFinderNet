using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL existing DbContext-related registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                d.ImplementationType == typeof(ApplicationDbContext)).ToList();
            foreach (var d in descriptorsToRemove) services.Remove(d);

            // Remove Npgsql specifically
            var npgsqlDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("Npgsql") == true ||
                d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                d.ImplementationFactory?.Method.ReturnType.FullName?.Contains("Npgsql") == true).ToList();
            foreach (var d in npgsqlDescriptors) services.Remove(d);

            // Also remove any pooled DbContext registrations
            var pooledDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("PooledDbContext") == true).ToList();
            foreach (var d in pooledDescriptors) services.Remove(d);

            // Register InMemory
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

            // Replace Redis with in-memory cache
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(StackExchange.Redis.ConnectionMultiplexer));
            if (redisDescriptor != null) services.Remove(redisDescriptor);

            services.AddScoped<ICacheService, FakeCacheService>();
            services.AddScoped<IAiService, MockAiService>();

            // Remove hosted services
            var hostedDescriptors = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
            foreach (var hd in hostedDescriptors) services.Remove(hd);

            var eqDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(EmailQueue));
            if (eqDescriptor != null) services.Remove(eqDescriptor);
            services.AddSingleton<EmailQueue>();

            // Remove existing auth registrations
            var authDescriptors = services.Where(d =>
                d.ServiceType == typeof(IAuthenticationSchemeProvider) ||
                d.ServiceType == typeof(IConfigureOptions<AuthenticationSchemeOptions>)).ToList();
            foreach (var ad in authDescriptors) services.Remove(ad);

            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();

            // Add test auth as the ONLY scheme
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Override the default authorization policy to use our Test scheme
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("sub", "test-user-id"),
            new Claim("email", "test@example.com"),
            new Claim("email_verified", "true"),
            new Claim(ClaimTypes.Role, "Applicant")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, string> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        _cache[key] = System.Text.Json.JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keys = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keys) _cache.Remove(key);
        return Task.CompletedTask;
    }
}
