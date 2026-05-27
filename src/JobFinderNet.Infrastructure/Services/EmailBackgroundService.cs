using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Infrastructure.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        EmailQueue queue,
        IServiceProvider services,
        ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email background service started");

        await foreach (var message in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _services.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await sender.SendAsync(message, stoppingToken);
                _logger.LogInformation("Email sent to {To}: {Subject}", message.To, message.Subject);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", message.To, message.Subject);
            }
        }

        _logger.LogInformation("Email background service stopped");
    }
}
