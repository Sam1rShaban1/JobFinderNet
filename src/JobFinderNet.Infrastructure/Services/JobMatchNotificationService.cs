using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class JobMatchNotificationService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<JobMatchNotificationService> _logger;
    private DateTime _lastCheck = DateTime.UtcNow;

    public JobMatchNotificationService(IServiceProvider services, ILogger<JobMatchNotificationService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job match notification service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMatches(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job matches");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task ProcessMatches(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var matching = scope.ServiceProvider.GetRequiredService<IMatchingService>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

        var since = _lastCheck;
        _lastCheck = DateTime.UtcNow;

        var newJobs = await context.Jobs
            .Where(j => j.IsActive && j.PostedDate >= since)
            .AsNoTracking()
            .ToListAsync(ct);

        if (newJobs.Count == 0) return;

        var profiles = await context.UserProfiles
            .Where(p => p.EmailOnMatch && p.Skills.Count > 0)
            .ToListAsync(ct);

        foreach (var profile in profiles)
        {
            foreach (var job in newJobs)
            {
                var score = await matching.CalculateMatchScore(job, profile);
                if (score < profile.MinimumMatchScore) continue;

                if (profile.EmailFrequency == "immediate")
                {
                    var user = await userManager.FindByIdAsync(profile.UserId);
                    if (user?.Email == null) continue;

                    await notifications.SendMatchNotificationAsync(
                        user.Email,
                        $"{user.FirstName ?? user.UserName ?? "there"}",
                        job.Title,
                        job.CompanyName,
                        score,
                        job.ApplyLink ?? $"http://localhost/jobs/{job.Id}"
                    );
                }
                else
                {
                    context.PendingDigests.Add(new PendingDigest
                    {
                        UserId = profile.UserId,
                        JobId = job.Id,
                        Job = job,
                        MatchScore = score,
                        JobTitle = job.Title,
                        CompanyName = job.CompanyName,
                        Location = job.Location,
                        Salary = job.Salary,
                        EmailFrequency = profile.EmailFrequency,
                    });
                }
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
