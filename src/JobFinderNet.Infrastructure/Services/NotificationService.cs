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

    public async Task SendMatchNotificationAsync(string email, string userName, string jobTitle, string companyName, int score, string applyLink)
    {
        var subject = $"New Job Match: {jobTitle} at {companyName} ({score}% match)";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px;">
                <h2 style="color: #1a1a2e;">Job Match Found!</h2>
                <p>Hi {userName},</p>
                <p>We found a new job that matches your profile:</p>
                <div style="background: #f5f5f5; border-radius: 8px; padding: 20px; margin: 16px 0;">
                    <h3 style="margin: 0 0 8px;">{jobTitle}</h3>
                    <p style="color: #666;">{companyName}</p>
                    <p style="font-size: 24px; font-weight: bold; color: #16a34a;">{score}% match</p>
                </div>
                <a href="{applyLink}" style="display: inline-block; padding: 12px 24px; background: #17171c; color: white; text-decoration: none; border-radius: 32px; font-weight: 500;">View Job</a>
                <hr style="border: 1px solid #eee; margin-top: 24px;" />
                <p style="color: #888; font-size: 12px;">JobFinder — Find your next opportunity</p>
            </div>
            """;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = email,
            Subject = subject,
            Body = body,
        });
    }

    public async Task SendDigestAsync(string email, string userName, List<PendingDigest> matches, string frequency)
    {
        var label = frequency == "weekly" ? "Weekly" : "Daily";
        var subject = $"{label} Job Digest — {matches.Count} new matches";

        var items = string.Join("\n", matches.Select(m => $"""
            <div style="background: #f5f5f5; border-radius: 8px; padding: 16px; margin-bottom: 12px;">
                <h4 style="margin: 0 0 4px;"><a href="{m.Job.ApplyLink ?? $"http://localhost/jobs/{m.JobId}"}" style="color: #1863dc; text-decoration: none;">{m.JobTitle}</a></h4>
                <p style="color: #666; margin: 0 0 4px;">{m.CompanyName}{(!string.IsNullOrEmpty(m.Location) ? $" — {m.Location}" : "")}</p>
                <p style="font-size: 14px; color: #888; margin: 0;">{(!string.IsNullOrEmpty(m.Salary) ? $"{m.Salary} | " : "")}{m.MatchScore}% match</p>
            </div>
            """));

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px;">
                <h2 style="color: #1a1a2e;">{label} Job Matches</h2>
                <p>Hi {userName},</p>
                <p>Here are the jobs that matched your profile this {frequency} period:</p>
                {items}
                <hr style="border: 1px solid #eee;" />
                <p style="color: #888; font-size: 12px;">JobFinder — Find your next opportunity</p>
            </div>
            """;

        await _emailQueue.EnqueueAsync(new EmailMessage
        {
            To = email,
            Subject = subject,
            Body = body,
        });
    }
}
