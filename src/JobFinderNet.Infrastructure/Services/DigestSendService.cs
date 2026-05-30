using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class DigestSendService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DigestSendService> _logger;

    public DigestSendService(IServiceProvider services, ILogger<DigestSendService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Digest send service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                if (now.Hour == 8 && now.Minute < 5)
                {
                    await SendDigests("daily", stoppingToken);
                }

                if (now.Hour == 8 && now.Minute < 5 && now.DayOfWeek == DayOfWeek.Monday)
                {
                    await SendDigests("weekly", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending digests");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task SendDigests(string frequency, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<JobFinderNet.Core.Models.ApplicationUser>>();

        var pending = await context.PendingDigests
            .Include(d => d.Job)
            .Where(d => d.EmailFrequency == frequency)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        var grouped = pending.GroupBy(d => d.UserId);

        foreach (var group in grouped)
        {
            var user = await userManager.FindByIdAsync(group.Key);
            if (user?.Email == null) continue;

            await notifications.SendDigestAsync(
                user.Email,
                $"{user.FirstName ?? user.UserName ?? "there"}",
                group.ToList(),
                frequency
            );
        }

        context.PendingDigests.RemoveRange(pending);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Sent {Count} {Freq} digest(s) to {Users} user(s)",
            pending.Count, frequency, grouped.Count());
    }
}
