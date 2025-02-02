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

    public async Task<bool> HasApplied(string userId, int jobId) =>
        await _context.Applications.AnyAsync(a => 
            a.ApplicantId == userId && a.JobId == jobId);

    public async Task AddAsync(JobApplication application)
    {
        await _context.Applications.AddAsync(application);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JobApplication>> GetUserApplications(string userId) =>
        await _context.Applications
            .Include(a => a.Job)
            .Where(a => a.ApplicantId == userId)
            .ToListAsync();

    public async Task<List<JobApplication>> GetJobApplications(int jobId) =>
        await _context.Applications
            .Include(a => a.Applicant)
            .Where(a => a.JobId == jobId)
            .ToListAsync();
} 