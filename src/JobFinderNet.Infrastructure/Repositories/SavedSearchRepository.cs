using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Repositories;

public class SavedSearchRepository : ISavedSearchRepository
{
    private readonly ApplicationDbContext _context;

    public SavedSearchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SavedSearch>> GetUserSavedSearchesAsync(string userId)
    {
        return await _context.SavedSearches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavedSearch?> GetByIdAsync(int id)
    {
        return await _context.SavedSearches.FindAsync(id);
    }

    public async Task<SavedSearch?> GetByIdForUserAsync(int id, string userId)
    {
        return await _context.SavedSearches
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task AddAsync(SavedSearch savedSearch)
    {
        _context.SavedSearches.Add(savedSearch);
    }

    public void Update(SavedSearch savedSearch)
    {
        _context.SavedSearches.Update(savedSearch);
    }

    public void Remove(SavedSearch savedSearch)
    {
        _context.SavedSearches.Remove(savedSearch);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
