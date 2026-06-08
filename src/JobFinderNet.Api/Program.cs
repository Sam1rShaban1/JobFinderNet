using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Api.Middleware;
using Scalar.AspNetCore;
using System.Security.Claims;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Redis distributed cache
var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string not found.");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "JobFinderNet:";
});

builder.Services.AddSingleton(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IJobRepository>(sp =>
{
    var inner = sp.GetRequiredService<JobRepository>();
    var cache = sp.GetRequiredService<ICacheService>();
    return new CachedJobRepository(inner, cache);
});
builder.Services.AddScoped<JobRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddScoped<IEmailService, SmtpEmailSender>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<JobMatchNotificationService>();
builder.Services.AddHostedService<DigestSendService>();

// JSearch job sync
builder.Services.Configure<JSearchOptions>(builder.Configuration.GetSection(JSearchOptions.SectionName));
builder.Services.AddHttpClient<IJSearchJobService, JSearchJobService>();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins("https://jobfindernet.azurewebsites.net")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var clerkAuthority = builder.Configuration["Clerk:Authority"]
    ?? throw new InvalidOperationException("Clerk:Authority not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = clerkAuthority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = clerkAuthority,
            ValidateAudience = false,
            ValidateLifetime = true,
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                try
                {
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<ApplicationUser>>();
                    var sub = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? context.Principal?.FindFirst("sub")?.Value;
                    if (sub == null) return;

                    var appUser = await userManager.FindByIdAsync(sub);
                    if (appUser == null)
                    {
                        var email = context.Principal?.FindFirst("email")?.Value ?? $"{sub}@clerk.dev";
                        appUser = new ApplicationUser
                        {
                            Id = sub,
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                        };
                        var result = await userManager.CreateAsync(appUser);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(appUser, "Applicant");
                        }
                    }

                    var roles = await userManager.GetRolesAsync(appUser);
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    var identity = context.Principal?.Identity as ClaimsIdentity;
                    logger.LogInformation("OnTokenValidated: sub={Sub}, roles={Roles}, identityNull={IsNull}",
                        sub, string.Join(",", roles), identity == null);
                    if (identity != null)
                    {
                        if (identity.FindFirst(ClaimTypes.NameIdentifier) == null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                        }
                        foreach (var role in roles)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "OnTokenValidated failed");
                }
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning(context.Exception, "Clerk JWT validation failed");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseRateLimiter();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health checks
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/api/ready", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "ready" });
    }
    catch
    {
        return Results.StatusCode(503);
    }
});

app.MapFallbackToFile("index.html");

try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        await context.Database.MigrateAsync();
        await RoleInitializer.Initialize(services);
        await DataSeeder.SeedData(services);

        logger.LogInformation("Database initialization completed");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the application.");
}

app.Run();
