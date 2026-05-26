using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendApplicationSubmittedAsync(string applicantEmail, string jobTitle)
    {
        _logger.LogInformation(
            "[Notification] Application submitted - Applicant: {Email}, Job: {JobTitle}",
            applicantEmail, jobTitle);
        return Task.CompletedTask;
    }

    public Task SendApplicationStatusChangedAsync(string applicantEmail, string jobTitle, ApplicationStatus newStatus)
    {
        _logger.LogInformation(
            "[Notification] Application status changed - Applicant: {Email}, Job: {JobTitle}, Status: {Status}",
            applicantEmail, jobTitle, newStatus);
        return Task.CompletedTask;
    }

    public Task SendJobPostedAsync(string employerEmail, string jobTitle)
    {
        _logger.LogInformation(
            "[Notification] Job posted - Employer: {Email}, Job: {JobTitle}",
            employerEmail, jobTitle);
        return Task.CompletedTask;
    }
}
