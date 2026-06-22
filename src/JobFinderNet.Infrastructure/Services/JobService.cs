using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyProfileRepository _companyProfileRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobService(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        ICompanyProfileRepository companyProfileRepository,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _companyProfileRepository = companyProfileRepository;
        _userManager = userManager;
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _jobRepository.GetByIdAsync(id);
    }

    public async Task<PaginatedList<Job>> GetPaginatedJobsAsync(int page, int pageSize)
    {
        return await _jobRepository.GetPaginatedJobsAsync(page, pageSize);
    }

    public async Task<List<Job>> SearchJobsAsync(string query)
    {
        return await _jobRepository.SearchJobsAsync(query);
    }

    public async Task<List<Job>> GetEmployerJobsAsync(string employerId)
    {
        return await _jobRepository.GetEmployerJobsAsync(employerId);
    }

    public async Task<Job> CreateJobAsync(CreateJobDto dto, string employerId)
    {
        var employer = await _userManager.FindByIdAsync(employerId)
            ?? throw new InvalidOperationException("User not found");

        var companyProfile = await _companyProfileRepository.GetByClaimedUserIdAsync(employerId);

        var job = new Job
        {
            Title = dto.Title,
            Description = dto.Description,
            CompanyName = companyProfile?.Name ?? dto.CompanyName,
            CompanyProfileId = companyProfile?.Id,
            EmployerLogo = dto.EmployerLogo,
            EmployerWebsite = dto.EmployerWebsite,
            Location = dto.Location,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            JobType = dto.JobType,
            Salary = dto.Salary,
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            SalaryCurrency = dto.SalaryCurrency,
            SalaryPeriod = dto.SalaryPeriod,
            ExperienceRequired = dto.ExperienceRequired,
            RequiredExperienceYears = dto.RequiredExperienceYears,
            SeniorityLevel = dto.SeniorityLevel,
            Industry = dto.Industry ?? companyProfile?.Industry,
            JobFunction = dto.JobFunction,
            WorkArrangement = dto.WorkArrangement,
            ApplyLink = dto.ApplyLink,
            IsRemote = dto.IsRemote,
            EducationRequired = dto.EducationRequired,
            ContractDuration = dto.ContractDuration,
            RequiredTechnologies = dto.RequiredTechnologies,
            PreferredTechnologies = dto.PreferredTechnologies,
            SoftSkills = dto.SoftSkills,
            Benefits = dto.Benefits,
            Methodologies = dto.Methodologies,
            HighlightsQualifications = dto.HighlightsQualifications,
            HighlightsResponsibilities = dto.HighlightsResponsibilities,
            HighlightsBenefits = dto.HighlightsBenefits,
            EmployerId = employerId,
            Employer = employer
        };

        await _jobRepository.CreateJobAsync(job);
        return job;
    }

    public async Task<Job?> UpdateJobAsync(int id, CreateJobDto dto, string employerId)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null || job.EmployerId != employerId)
            return null;

        var companyProfile = await _companyProfileRepository.GetByClaimedUserIdAsync(employerId);

        job.Title = dto.Title;
        job.Description = dto.Description;
        job.CompanyName = companyProfile?.Name ?? dto.CompanyName;
        job.CompanyProfileId = companyProfile?.Id;
        job.EmployerLogo = dto.EmployerLogo;
        job.EmployerWebsite = dto.EmployerWebsite;
        job.Location = dto.Location;
        job.City = dto.City;
        job.State = dto.State;
        job.Country = dto.Country;
        job.JobType = dto.JobType;
        job.Salary = dto.Salary;
        job.SalaryMin = dto.SalaryMin;
        job.SalaryMax = dto.SalaryMax;
        job.SalaryCurrency = dto.SalaryCurrency;
        job.SalaryPeriod = dto.SalaryPeriod;
        job.ExperienceRequired = dto.ExperienceRequired;
        job.RequiredExperienceYears = dto.RequiredExperienceYears;
        job.SeniorityLevel = dto.SeniorityLevel;
        job.Industry = dto.Industry ?? companyProfile?.Industry;
        job.JobFunction = dto.JobFunction;
        job.WorkArrangement = dto.WorkArrangement;
        job.ApplyLink = dto.ApplyLink;
        job.IsRemote = dto.IsRemote;
        job.EducationRequired = dto.EducationRequired;
        job.ContractDuration = dto.ContractDuration;
        job.RequiredTechnologies = dto.RequiredTechnologies;
        job.PreferredTechnologies = dto.PreferredTechnologies;
        job.SoftSkills = dto.SoftSkills;
        job.Benefits = dto.Benefits;
        job.Methodologies = dto.Methodologies;
        job.HighlightsQualifications = dto.HighlightsQualifications;
        job.HighlightsResponsibilities = dto.HighlightsResponsibilities;
        job.HighlightsBenefits = dto.HighlightsBenefits;

        await _jobRepository.UpdateJobAsync(job);
        return job;
    }

    public async Task DeleteJobAsync(int id, string employerId)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null || job.EmployerId != employerId)
            throw new KeyNotFoundException("Job not found");

        await _jobRepository.DeleteJobAsync(id);
    }

    public async Task ToggleJobStatusAsync(int id, string employerId)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null || job.EmployerId != employerId)
            throw new KeyNotFoundException("Job not found");

        await _jobRepository.ToggleJobStatusAsync(id);
    }

    public async Task<List<Job>> GetSimilarJobsAsync(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null) return new List<Job>();

        return await _jobRepository.GetSimilarJobsAsync(
            id, job.Industry, job.CompanyName, job.RequiredTechnologies);
    }

    public async Task PopulateTechnologiesAsync()
    {
        var jobs = await _jobRepository.GetJobsWithoutTechnologiesAsync();
        foreach (var job in jobs)
        {
            var (required, preferred) = JSearchJobService.ExtractTechnologies($"{job.Title} {job.Description}");
            job.RequiredTechnologies = required;
            job.PreferredTechnologies = preferred;
            await _jobRepository.UpdateJobAsync(job);
        }
    }

    public async Task<List<Application>> GetJobApplicationsAsync(int jobId, string employerId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null || job.EmployerId != employerId)
            throw new KeyNotFoundException("Job not found");

        return (List<Application>)await _applicationRepository.GetJobApplications(jobId);
    }
}
