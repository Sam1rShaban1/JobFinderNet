using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatisticsService(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _userManager = userManager;
    }

    public async Task<JobStatisticsDto> GetDashboardStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);

        var jobs = await _jobRepository.GetAllActiveJobsAsync();
        var applications = await _applicationRepository.GetByJobIdsAsync(jobs.Select(j => j.Id).ToList());

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
            TotalActiveJobs = jobs.Count,
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
        var jobs = await _jobRepository.GetEmployerJobsAsync(employerId);
        return jobs.Count;
    }

    public async Task<Dictionary<string, int>> GetApplicationsByJobAsync(string employerId)
    {
        var jobs = await _jobRepository.GetEmployerJobsAsync(employerId);
        var jobIds = jobs.Select(j => j.Id).ToList();
        var applications = await _applicationRepository.GetByJobIdsAsync(jobIds);

        return jobs.ToDictionary(
            j => j.Title,
            j => applications.Count(a => a.JobId == j.Id));
    }

    public async Task<EmployerDashboardDto> GetEmployerDashboardAsync(string employerId)
    {
        var now = DateTime.UtcNow;
        var sixMonthsAgo = now.AddMonths(-6);

        var employerJobs = await _jobRepository.GetEmployerJobsAsync(employerId);
        var jobIds = employerJobs.Select(j => j.Id).ToList();
        var applications = await _applicationRepository.GetByJobIdsAsync(jobIds);

        var monthlyPostings = employerJobs
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

        var topJobs = employerJobs
            .Select(j => new JobApplicationCount
            {
                JobId = j.Id,
                Title = j.Title,
                ApplicationCount = applications.Count(a => a.JobId == j.Id)
            })
            .OrderByDescending(x => x.ApplicationCount)
            .Take(5)
            .ToList();

        return new EmployerDashboardDto
        {
            TotalJobs = employerJobs.Count,
            ActiveJobs = employerJobs.Count(j => j.IsActive),
            TotalApplications = applications.Count,
            ApplicationsByStatus = applications
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            TopJobs = topJobs,
            MonthlyPostings = monthlyPostings
        };
    }

    public async Task<PublicStatisticsDto> GetPublicStatisticsAsync()
    {
        var totalJobs = await _jobRepository.GetCountAsync();
        var jobsByType = await _jobRepository.GetJobsByTypeAsync();
        var allJobs = await _jobRepository.GetAllActiveJobsAsync();

        var jobsWithTech = allJobs.Count(j =>
            j.RequiredTechnologies.Count > 0 || j.PreferredTechnologies.Count > 0);

        var allTech = allJobs
            .SelectMany(j => j.RequiredTechnologies)
            .Concat(allJobs.SelectMany(j => j.PreferredTechnologies))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .Count();

        return new PublicStatisticsDto
        {
            TotalJobs = totalJobs,
            TotalUsers = (await _userManager.GetUsersInRoleAsync("Applicant")).Count
                       + (await _userManager.GetUsersInRoleAsync("Employer")).Count,
            TotalApplications = await _applicationRepository.GetCountAsync(),
            JobsWithTech = jobsWithTech,
            TotalTechnologies = allTech,
            JobsByType = jobsByType
        };
    }
}
