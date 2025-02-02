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

    public async Task<bool> HasUserAppliedToJob(string userId, int jobId)
    {
        return await _context.Applications
            .AnyAsync(a => a.ApplicantId == userId && a.JobId == jobId);
    }

    public async Task<bool> AddAsync(Application application)
    {
        try
        {
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Application>> GetUserApplicationsAsync(string userId)
    {
        return await _context.Applications
            .Include(a => a.Job)
            .Where(a => a.ApplicantId == userId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
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