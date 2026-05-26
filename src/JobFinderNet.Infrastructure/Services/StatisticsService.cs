using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatisticsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<JobStatisticsDto> GetDashboardStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);

        var jobs = await _context.Jobs.ToListAsync();
        var applications = await _context.Applications.ToListAsync();
        var users = await _context.Users.ToListAsync();

        var employerCount = (await _userManager.GetUsersInRoleAsync("Employer")).Count;
        var applicantCount = (await _userManager.GetUsersInRoleAsync("Applicant")).Count;

        var monthlyPostings = jobs
            .Where(j => j.PostedDate >= sixMonthsAgo)
            .GroupBy(j => new { j.PostedDate.Year, j.PostedDate.Month })
            .Select(g => new MonthlyJobPosting
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return new JobStatisticsDto
        {
            TotalActiveJobs = jobs.Count(j => j.IsActive),
            TotalApplications = applications.Count,
            TotalEmployers = employerCount,
            TotalApplicants = applicantCount,
            JobsByType = jobs.GroupBy(j => j.JobType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ApplicationsByStatus = applications.GroupBy(a => a.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            MonthlyJobPostings = monthlyPostings
        };
    }

    public async Task<int> GetEmployerJobCountAsync(string employerId)
    {
        return await _context.Jobs.CountAsync(j => j.EmployerId == employerId);
    }

    public async Task<Dictionary<string, int>> GetApplicationsByJobAsync(string employerId)
    {
        return await _context.Jobs
            .Where(j => j.EmployerId == employerId)
            .Select(j => new
            {
                JobTitle = j.Title,
                ApplicationCount = j.Applications.Count
            })
            .ToDictionaryAsync(x => x.JobTitle, x => x.ApplicationCount);
    }
}
