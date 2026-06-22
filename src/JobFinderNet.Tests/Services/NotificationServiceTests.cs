using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Services;

public class NotificationServiceTests
{
    private readonly EmailQueue _emailQueue;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _emailQueue = new EmailQueue();
        _service = new NotificationService(_emailQueue);
    }

    private async Task<EmailMessage> DequeueMessage()
    {
        var cts = new CancellationTokenSource(1000);
        await foreach (var msg in _emailQueue.ReadAllAsync(cts.Token))
            return msg;
        throw new TimeoutException("No message enqueued");
    }

    [Fact]
    public async Task SendApplicationSubmittedAsync_EnqueuesEmail()
    {
        await _service.SendApplicationSubmittedAsync("app@test.com", "Alice", "Engineer", "Corp");

        var msg = await DequeueMessage();
        Assert.Equal("app@test.com", msg.To);
        Assert.Contains("Engineer", msg.Subject);
        Assert.Contains("Application Submitted", msg.Body);
    }

    [Fact]
    public async Task SendApplicationStatusChangedAsync_Accepted_EnqueuesEmail()
    {
        await _service.SendApplicationStatusChangedAsync("app@test.com", "Alice", "Engineer", ApplicationStatus.Accepted);

        var msg = await DequeueMessage();
        Assert.Equal("app@test.com", msg.To);
        Assert.Contains("Application Update", msg.Subject);
        Assert.Contains("Accepted", msg.Body);
    }

    [Fact]
    public async Task SendApplicationStatusChangedAsync_Rejected_EnqueuesEmail()
    {
        await _service.SendApplicationStatusChangedAsync("app@test.com", "Bob", "Analyst", ApplicationStatus.Rejected);

        var msg = await DequeueMessage();
        Assert.Equal("app@test.com", msg.To);
        Assert.Contains("Not Selected", msg.Body);
    }

    [Fact]
    public async Task SendApplicationStatusChangedAsync_Screening_EnqueuesEmail()
    {
        await _service.SendApplicationStatusChangedAsync("app@test.com", "Bob", "Analyst", ApplicationStatus.Screening);

        var msg = await DequeueMessage();
        Assert.Equal("app@test.com", msg.To);
        Assert.Contains("Screening", msg.Body);
    }

    [Fact]
    public async Task SendJobPostedAsync_EnqueuesEmail()
    {
        await _service.SendJobPostedAsync("emp@test.com", "Employer Inc", "Dev");

        var msg = await DequeueMessage();
        Assert.Equal("emp@test.com", msg.To);
        Assert.Contains("Job Posted", msg.Subject);
    }

    [Fact]
    public async Task SendNewApplicationToEmployerAsync_EnqueuesEmail()
    {
        await _service.SendNewApplicationToEmployerAsync("emp@test.com", "Bob", "Alice", "Engineer");

        var msg = await DequeueMessage();
        Assert.Equal("emp@test.com", msg.To);
        Assert.Contains("New Application", msg.Subject);
        Assert.Contains("Alice", msg.Body);
    }

    [Fact]
    public async Task SendMatchNotificationAsync_EnqueuesEmail()
    {
        await _service.SendMatchNotificationAsync("app@test.com", "Alice", "Dev", "Corp", 85, "http://example.com/apply");

        var msg = await DequeueMessage();
        Assert.Equal("app@test.com", msg.To);
        Assert.Contains("85%", msg.Subject);
        Assert.Contains("View Job", msg.Body);
    }

    [Fact]
    public async Task SendDigestAsync_Daily_EnqueuesEmail()
    {
        var matches = new List<PendingDigest>
        {
            new() { JobId = 1, JobTitle = "Dev", CompanyName = "Corp", MatchScore = 90, UserId = "u1", EmailFrequency = "daily", Job = new Job { Id = 1, Title = "Dev", Description = "D", CompanyName = "Corp", Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry", EmployerId = "e1", IsActive = true, PostedDate = DateTime.UtcNow } }
        };

        await _service.SendDigestAsync("app@test.com", "Alice", matches, "daily");

        var msg = await DequeueMessage();
        Assert.Equal("app@test.com", msg.To);
        Assert.Contains("Daily", msg.Subject);
        Assert.Contains("Dev", msg.Body);
    }

    [Fact]
    public async Task SendDigestAsync_Weekly_EnqueuesEmail()
    {
        var matches = new List<PendingDigest>
        {
            new() { JobId = 1, JobTitle = "Dev", CompanyName = "Corp", MatchScore = 90, UserId = "u1", EmailFrequency = "weekly", Job = new Job { Id = 1, Title = "Dev", Description = "D", CompanyName = "Corp", Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry", EmployerId = "e1", IsActive = true, PostedDate = DateTime.UtcNow } }
        };

        await _service.SendDigestAsync("app@test.com", "Alice", matches, "weekly");

        var msg = await DequeueMessage();
        Assert.Contains("Weekly", msg.Subject);
    }

    [Fact]
    public async Task SendDigestAsync_MultipleMatches_EnqueuesEmail()
    {
        var matches = new List<PendingDigest>
        {
            new() { JobId = 1, JobTitle = "Dev", CompanyName = "Corp", MatchScore = 90, UserId = "u1", EmailFrequency = "daily", Job = new Job { Id = 1, Title = "Dev", Description = "D", CompanyName = "Corp", Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry", EmployerId = "e1", IsActive = true, PostedDate = DateTime.UtcNow } },
            new() { JobId = 2, JobTitle = "Analyst", CompanyName = "Inc", MatchScore = 80, UserId = "u1", EmailFrequency = "daily", Job = new Job { Id = 2, Title = "Analyst", Description = "D", CompanyName = "Inc", Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry", EmployerId = "e1", IsActive = true, PostedDate = DateTime.UtcNow } }
        };

        await _service.SendDigestAsync("app@test.com", "Alice", matches, "daily");

        var msg = await DequeueMessage();
        Assert.Contains("2 new matches", msg.Subject);
        Assert.Contains("Analyst", msg.Body);
    }
}
