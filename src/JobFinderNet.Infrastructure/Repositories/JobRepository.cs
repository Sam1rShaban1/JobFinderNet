using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;

    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<Job>> GetActiveJobsAsync(int page, int pageSize)
    {
        return await _context.Jobs
            .Where(j => j.IsActive)
            .OrderBy(j => j.PostedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalActiveJobsCount()
    {
        return await _context.Jobs
            .Where(j => j.IsActive)
            .CountAsync();
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
            existingJob.CompanyName = job.CompanyName;
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
                 j.CompanyName.Contains(query)))
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

    public async Task<List<Job>> GetSimilarJobsAsync(int jobId, string? industry, string companyName, List<string> technologies, int limit = 6)
    {
        return await _context.Jobs
            .Where(j => j.Id != jobId && j.IsActive && (
                (j.Industry != null && j.Industry == industry) ||
                (j.CompanyName == companyName) ||
                j.RequiredTechnologies.Any(t => technologies.Contains(t))
            ))
            .OrderByDescending(j => j.RequiredTechnologies.Count(t => technologies.Contains(t)))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Job>> GetJobsWithoutTechnologiesAsync()
    {
        return await _context.Jobs
            .Where(j => j.RequiredTechnologies.Count == 0 && j.PreferredTechnologies.Count == 0)
            .ToListAsync();
    }

    public async Task<List<Job>> GetAllActiveJobsAsync()
    {
        return await _context.Jobs
            .Where(j => j.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Jobs.CountAsync();
    }

    public async Task<Dictionary<string, int>> GetJobsByTypeAsync()
    {
        return await _context.Jobs
            .GroupBy(j => j.JobType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);
    }
}
