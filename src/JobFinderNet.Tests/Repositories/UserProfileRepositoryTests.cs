using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class UserProfileRepositoryTests
{
    [Fact]
    public async Task GetByUserIdAsync_ReturnsProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new UserProfileRepository(context);

        context.UserProfiles.Add(new UserProfile { UserId = "u1" });
        await context.SaveChangesAsync();

        var result = await repo.GetByUserIdAsync("u1");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_NotFound_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new UserProfileRepository(context);

        var result = await repo.GetByUserIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new UserProfileRepository(context);

        await repo.AddAsync(new UserProfile { UserId = "u1" });
        await repo.SaveChangesAsync();

        Assert.Equal(1, await context.UserProfiles.CountAsync());
    }

    [Fact]
    public async Task Update_UpdatesProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new UserProfileRepository(context);

        var profile = new UserProfile { UserId = "u1" };
        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();

        profile.PreferredLocation = "Remote";
        repo.Update(profile);
        await repo.SaveChangesAsync();

        var saved = await context.UserProfiles.FirstAsync(p => p.UserId == "u1");
        Assert.Equal("Remote", saved.PreferredLocation);
    }
}
