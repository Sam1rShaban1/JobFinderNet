using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Repositories;

public class SavedJobRepository : ISavedJobRepository
{
    private readonly ApplicationDbContext _context;

    public SavedJobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SavedJob>> GetUserSavedJobsAsync(string userId)
    {
        return await _context.SavedJobs
            .Include(s => s.Job)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SavedDate)
            .ToListAsync();
    }

    public async Task<List<int>> GetUserSavedJobIdsAsync(string userId)
    {
        return await _context.SavedJobs
            .Where(s => s.UserId == userId)
            .Select(s => s.JobId)
            .ToListAsync();
    }

    public async Task<SavedJob?> GetAsync(string userId, int jobId)
    {
        return await _context.SavedJobs
            .FirstOrDefaultAsync(s => s.UserId == userId && s.JobId == jobId);
    }

    public async Task<bool> ExistsAsync(string userId, int jobId)
    {
        return await _context.SavedJobs
            .AnyAsync(s => s.UserId == userId && s.JobId == jobId);
    }

    public async Task AddAsync(SavedJob savedJob)
    {
        _context.SavedJobs.Add(savedJob);
    }

    public void Remove(SavedJob savedJob)
    {
        _context.SavedJobs.Remove(savedJob);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
