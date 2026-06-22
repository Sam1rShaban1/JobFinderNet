using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Repositories;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Repositories;

public class SavedSearchRepositoryTests
{
    [Fact]
    public async Task GetUserSavedSearchesAsync_ReturnsSearches()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        context.SavedSearches.AddRange(
            new SavedSearch { UserId = "u1", Name = "Search 1" },
            new SavedSearch { UserId = "u1", Name = "Search 2" }
        );
        await context.SaveChangesAsync();

        var results = await repo.GetUserSavedSearchesAsync("u1");

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetUserSavedSearchesAsync_NoResults_ReturnsEmpty()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var results = await repo.GetUserSavedSearchesAsync("u1");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSearch()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var search = new SavedSearch { UserId = "u1", Name = "My Search" };
        context.SavedSearches.Add(search);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(search.Id);

        Assert.NotNull(result);
        Assert.Equal("My Search", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdForUserAsync_OwnSearch_ReturnsSearch()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var search = new SavedSearch { UserId = "u1", Name = "My Search" };
        context.SavedSearches.Add(search);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdForUserAsync(search.Id, "u1");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdForUserAsync_OtherUser_ReturnsNull()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var search = new SavedSearch { UserId = "u1", Name = "My Search" };
        context.SavedSearches.Add(search);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdForUserAsync(search.Id, "u2");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsSearch()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        await repo.AddAsync(new SavedSearch { UserId = "u1", Name = "New Search" });
        await repo.SaveChangesAsync();

        Assert.Equal(1, await context.SavedSearches.CountAsync());
    }

    [Fact]
    public async Task Update_UpdatesSearch()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var search = new SavedSearch { UserId = "u1", Name = "Original" };
        context.SavedSearches.Add(search);
        await context.SaveChangesAsync();

        search.Name = "Updated";
        repo.Update(search);
        await repo.SaveChangesAsync();

        var saved = await context.SavedSearches.FindAsync(search.Id);
        Assert.Equal("Updated", saved!.Name);
    }

    [Fact]
    public async Task Remove_RemovesSearch()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var search = new SavedSearch { UserId = "u1", Name = "To Delete" };
        context.SavedSearches.Add(search);
        await context.SaveChangesAsync();

        repo.Remove(search);
        await repo.SaveChangesAsync();

        Assert.Equal(0, await context.SavedSearches.CountAsync());
    }

    [Fact]
    public async Task GetUserProfileAsync_ReturnsProfile()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new SavedSearchRepository(context);

        var profile = new UserProfile { UserId = "u1" };
        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();

        var result = await repo.GetUserProfileAsync("u1");

        Assert.NotNull(result);
    }
}
