using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class ApplicationNoteRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddNote()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationNoteRepository(context);

        var applicant = TestDbContextFactory.CreateTestUser("app1", "app@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(1, "Job", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);

        var app = TestDbContextFactory.CreateTestApplication(1, 1, applicant.Id);
        context.Applications.Add(app);
        await context.SaveChangesAsync();

        var note = new ApplicationNote
        {
            ApplicationId = 1,
            UserId = employer.Id,
            Content = "Test note content",
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(note);

        var saved = await context.ApplicationNotes.FirstOrDefaultAsync(n => n.Id == note.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test note content", saved.Content);
    }

    [Fact]
    public async Task GetByApplicationIdAsync_ReturnsNotes()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationNoteRepository(context);

        var applicant = TestDbContextFactory.CreateTestUser("app1", "app@test.com", "Applicant");
        var employer = TestDbContextFactory.CreateTestUser("emp1", "emp@test.com", "Employer");
        context.Users.AddRange(applicant, employer);

        var job = TestDbContextFactory.CreateTestJob(1, "Job", employer.Id);
        job.Employer = employer;
        context.Jobs.Add(job);

        var app = TestDbContextFactory.CreateTestApplication(1, 1, applicant.Id);
        context.Applications.Add(app);
        context.ApplicationNotes.AddRange(
            new ApplicationNote { ApplicationId = 1, UserId = employer.Id, Content = "Note 1", CreatedAt = DateTime.UtcNow },
            new ApplicationNote { ApplicationId = 1, UserId = employer.Id, Content = "Note 2", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var notes = await repo.GetByApplicationIdAsync(1);

        Assert.Equal(2, notes.Count);
    }

    [Fact]
    public async Task GetByApplicationIdAsync_NoNotes_ReturnsEmpty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new ApplicationNoteRepository(context);

        var notes = await repo.GetByApplicationIdAsync(999);

        Assert.Empty(notes);
    }
}
