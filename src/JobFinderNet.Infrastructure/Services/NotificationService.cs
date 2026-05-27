using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly EmailQueue _emailQueue;

    public NotificationService(EmailQueue emailQueue)
    {
        _emailQueue = emailQueue;
    }

    public async Task SendApplicationSubmittedAsync(string applicantEmail, string applicantName, string jobTitle, string companyName)
    {
        var subject = $"Application Received — {jobTitle}";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px;">
                <h2 style="color: #1a1a2e;">Application Submitted</h2>
                <p>Hi {applicantName},</p>
                <p>Your application for <strong>{jobTitle}</strong> at <strong>{companyName}</strong> has been received successfully.</p>
                <p>The employer will review your application and update you on the status.</p>
                <hr style="border: 1px solid #eee;" />
                <p style="color: #888; font-size: 12px;">JobFinder — Find your next opportunity</p>
            </div>
            """;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = applicantEmail,
            Subject = subject,
            Body = body,
        });
    }

    public async Task SendApplicationStatusChangedAsync(string applicantEmail, string applicantName, string jobTitle, ApplicationStatus newStatus)
    {
var statusLabel = newStatus switch
{
    ApplicationStatus.Accepted => "Accepted ✓",
    ApplicationStatus.Rejected => "Not Selected",
    _ => newStatus.ToString(),
};

        var subject = $"Application Update — {jobTitle}";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px;">
                <h2 style="color: #1a1a2e;">Application Status Update</h2>
                <p>Hi {applicantName},</p>
                <p>Your application for <strong>{jobTitle}</strong> has been updated to:</p>
                <p style="font-size: 18px; font-weight: bold; color: #16a34a;">{statusLabel}</p>
                <hr style="border: 1px solid #eee;" />
                <p style="color: #888; font-size: 12px;">JobFinder — Find your next opportunity</p>
            </div>
            """;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = applicantEmail,
            Subject = subject,
            Body = body,
        });
    }

    public async Task SendJobPostedAsync(string employerEmail, string employerName, string jobTitle)
    {
        var subject = $"Job Posted Successfully — {jobTitle}";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px;">
                <h2 style="color: #1a1a2e;">Job Posted</h2>
                <p>Hi {employerName},</p>
                <p>Your job listing for <strong>{jobTitle}</strong> is now live and visible to applicants.</p>
                <p>You'll receive notifications when candidates apply.</p>
                <hr style="border: 1px solid #eee;" />
                <p style="color: #888; font-size: 12px;">JobFinder — Find your next opportunity</p>
            </div>
            """;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = employerEmail,
            Subject = subject,
            Body = body,
        });
    }

    public async Task SendNewApplicationToEmployerAsync(string employerEmail, string employerName, string applicantName, string jobTitle)
    {
        var subject = $"New Application — {jobTitle}";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px;">
                <h2 style="color: #1a1a2e;">New Application Received</h2>
                <p>Hi {employerName},</p>
                <p><strong>{applicantName}</strong> has applied to your position <strong>{jobTitle}</strong>.</p>
                <p>Log in to review their application and update the status.</p>
                <hr style="border: 1px solid #eee;" />
                <p style="color: #888; font-size: 12px;">JobFinder — Find your next opportunity</p>
            </div>
            """;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = employerEmail,
            Subject = subject,
            Body = body,
        });
    }
}
