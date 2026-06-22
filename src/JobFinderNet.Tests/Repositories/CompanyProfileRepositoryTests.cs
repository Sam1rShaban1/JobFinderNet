using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class CompanyProfileRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        var profile = new CompanyProfile { Name = "Test Corp" };
        context.CompanyProfiles.Add(profile);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(profile.Id);

        Assert.NotNull(result);
        Assert.Equal("Test Corp", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        context.CompanyProfiles.Add(new CompanyProfile { Name = "Unique Name" });
        await context.SaveChangesAsync();

        var result = await repo.GetByNameAsync("Unique Name");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByNameAsync_NotFound_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        var result = await repo.GetByNameAsync("NonExistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByClaimedUserIdAsync_ReturnsProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        context.CompanyProfiles.Add(new CompanyProfile { Name = "Mine", ClaimedByUserId = "u1" });
        await context.SaveChangesAsync();

        var result = await repo.GetByClaimedUserIdAsync("u1");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByClaimedUserIdAsync_NotFound_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        var result = await repo.GetByClaimedUserIdAsync("u1");

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsMatching()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        context.CompanyProfiles.AddRange(
            new CompanyProfile { Name = "Alpha Corp" },
            new CompanyProfile { Name = "Beta Inc" }
        );
        await context.SaveChangesAsync();

        var results = await repo.SearchAsync("Alpha");

        Assert.Single(results);
        Assert.Equal("Alpha Corp", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_NoQuery_ReturnsAll()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        context.CompanyProfiles.AddRange(
            new CompanyProfile { Name = "Alpha Corp" },
            new CompanyProfile { Name = "Beta Inc" }
        );
        await context.SaveChangesAsync();

        var results = await repo.SearchAsync(null);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task AddAsync_SavesAndCanRetrieve()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        var profile = new CompanyProfile { Name = "New Corp" };
        await repo.AddAsync(profile);
        await repo.SaveChangesAsync();

        var saved = await context.CompanyProfiles.FirstOrDefaultAsync(c => c.Name == "New Corp");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Update_UpdatesProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new CompanyProfileRepository(context);

        var profile = new CompanyProfile { Name = "Old Name" };
        context.CompanyProfiles.Add(profile);
        await context.SaveChangesAsync();

        profile.Name = "New Name";
        repo.Update(profile);
        await repo.SaveChangesAsync();

        var saved = await context.CompanyProfiles.FindAsync(profile.Id);
        Assert.Equal("New Name", saved!.Name);
    }
}
