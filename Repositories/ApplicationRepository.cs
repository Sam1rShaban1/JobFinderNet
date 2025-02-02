using Microsoft.EntityFrameworkCore;
using JobFinderNet.Data;
using JobFinderNet.Models;

namespace JobFinderNet.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;
    
    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(Application application)
    {
        await _context.Applications.AddAsync(application);
        var saved = await _context.SaveChangesAsync();
        return saved > 0;
    }

    public async Task<List<Application>> GetJobApplications(int jobId)
    {
        return await _context.Applications
            .Include(a => a.Applicant)
            .Where(a => a.JobId == jobId)
            .ToListAsync();
    }

    public async Task<List<Application>> GetUserApplications(string userId)
    {
        return await _context.Applications
            .Include(a => a.Job)
            .Where(a => a.ApplicantId == userId)
            .ToListAsync();
    }

    public async Task<bool> HasApplied(string userId, int jobId)
    {
        return await _context.Applications
            .AnyAsync(a => a.ApplicantId == userId && a.JobId == jobId);
    }
} 