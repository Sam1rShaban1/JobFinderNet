using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Data;
using JobFinderNet.Models;
using System.Linq;

namespace JobFinderNet.Repositories;


public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;
    
    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> ApplyForJobAsync(int jobId, string userId)
    {
        // Complex logic: Check application limits, validate qualifications, etc.
        var existingApplication = await _context.Applications
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.ApplicantId == userId);
        
        if(existingApplication != null) return false;
        
        var application = new JobApplication
        {
            JobId = jobId,
            ApplicantId = userId,
            Status = ApplicationStatus.Pending
        };
        
        _context.Applications.Add(application);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task AddAsync(Job job) => await _context.Jobs.AddAsync(job);
    public async Task UpdateAsync(Job job) => _context.Jobs.Update(job);
    public async Task DeleteAsync(int id) => _context.Jobs.Remove(await GetByIdAsync(id));

    public async Task<List<Job>> GetActiveJobsAsync() => 
        await _context.Jobs.Where(j => j.IsActive).ToListAsync();

    public async Task CreateJobAsync(Job job) => await _context.Jobs.AddAsync(job);
    
    public async Task UpdateJobAsync(Job job) => _context.Jobs.Update(job);
    
    public async Task<bool> JobExists(int id) => 
        await _context.Jobs.AnyAsync(e => e.Id == id);

    public async Task<Job> GetByIdAsync(int id) => await _context.Jobs.FindAsync(id);

    public async Task ToggleJobStatusAsync(int id)
    {
        var job = await GetByIdAsync(id);
        if (job != null)
        {
            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PaginatedList<Job>> GetPaginatedJobsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Jobs
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.PostedDate);
        
        var count = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
                              
        return new PaginatedList<Job>(items, count, pageIndex, pageSize);
    }

    public async Task<List<Job>> SearchJobsAsync(string query)
    {
        return await _context.Jobs
            .Where(j => j.IsActive && 
                (j.Title.Contains(query) || j.Description.Contains(query)))
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }
} 