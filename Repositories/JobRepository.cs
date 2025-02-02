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
        var job = await _context.Jobs.FindAsync(jobId);
        var applicant = await _context.Users.FindAsync(userId);
        
        if (job == null || applicant == null) return false;
        
        var existingApplication = await _context.Applications
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.ApplicantId == userId);
        
        if (existingApplication != null) return false;
        
        var application = new JobApplication
        {
            JobId = jobId,
            Job = job,
            ApplicantId = userId,
            Applicant = applicant,
            Status = ApplicationStatus.Pending
        };
        
        _context.Applications.Add(application);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Applications)
                .ThenInclude(a => a.Applicant)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<List<Job>> GetActiveJobsAsync()
    {
        return await _context.Jobs
            .Include(j => j.Employer)
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<List<Job>> GetEmployerJobsAsync(string employerId)
    {
        return await _context.Jobs
            .Include(j => j.Applications)
            .Where(j => j.EmployerId == employerId)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task CreateJobAsync(Job job)
    {
        job.PostedDate = DateTime.UtcNow;
        job.IsActive = true;
        await _context.Jobs.AddAsync(job);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateJobAsync(Job job)
    {
        var existingJob = await _context.Jobs.FindAsync(job.Id);
        if (existingJob != null)
        {
            existingJob.Title = job.Title;
            existingJob.Description = job.Description;
            existingJob.Company = job.Company;
            existingJob.IsActive = job.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleJobStatusAsync(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job != null)
        {
            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> JobExists(int id)
    {
        return await _context.Jobs.AnyAsync(j => j.Id == id);
    }

    public async Task<PaginatedList<Job>> GetPaginatedJobsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Jobs
            .Include(j => j.Employer)
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
            .Include(j => j.Employer)
            .Where(j => j.IsActive && 
                (j.Title.Contains(query) || 
                 j.Description.Contains(query) || 
                 j.Company.Contains(query)))
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<List<Job>> GetRecentJobsAsync(int count)
    {
        return await _context.Jobs
            .Include(j => j.Employer)
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.PostedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task DeleteJobAsync(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
} 