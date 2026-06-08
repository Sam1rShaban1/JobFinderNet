using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<JobsController> _logger;
    private readonly ApplicationDbContext _context;

    public JobsController(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<JobsController> logger,
        ApplicationDbContext context)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var jobs = await _jobRepository.GetPaginatedJobsAsync(page, pageSize);
        return Ok(new
        {
            jobs.Items,
            jobs.PageIndex,
            jobs.TotalPages,
            jobs.TotalCount,
            jobs.HasPreviousPage,
            jobs.HasNextPage
        });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Job>> GetJob(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<Job>());

        var jobs = await _jobRepository.SearchJobsAsync(query);
        return Ok(jobs);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Job>> CreateJob(CreateJobDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var employerId = User.GetUserId()
            ?? throw new InvalidOperationException("User ID not found");

        var employer = await _userManager.FindByIdAsync(employerId);
        if (employer == null) return Unauthorized();

        var companyProfile = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.ClaimedByUserId == employerId);

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
        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<Job>> UpdateJob(int id, CreateJobDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var employerId = User.GetUserId()!;
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();
        if (job.EmployerId != employerId) return Forbid();

        var companyProfile = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.ClaimedByUserId == employerId);

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

        await _context.SaveChangesAsync();
        await _jobRepository.UpdateJobAsync(job);

        return Ok(job);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteJob(int id)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        await _jobRepository.DeleteJobAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/toggle")]
    [Authorize]
    public async Task<ActionResult> ToggleJobStatus(int id)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        await _jobRepository.ToggleJobStatusAsync(id);
        return NoContent();
    }

    [HttpGet("employer")]
    [Authorize]
    public async Task<ActionResult> GetEmployerJobs()
    {
        if (!User.HasRole("Employer"))
            return Forbid();

        var employerId = User.GetUserId()!;
        var jobs = await _jobRepository.GetEmployerJobsAsync(employerId);
        return Ok(jobs);
    }

    [HttpGet("{jobId}/applications")]
    [Authorize]
    public async Task<ActionResult> GetJobApplications(int jobId)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var apps = await _applicationRepository.GetJobApplications(jobId);
        return Ok(apps);
    }

    [HttpPost("populate-techs")]
    [Authorize]
    public async Task<ActionResult> PopulateTechnologies()
    {
        if (!User.HasRole("Admin"))
            return Forbid();

        var jobs = await _context.Jobs
            .Where(j => j.RequiredTechnologies.Count == 0 && j.PreferredTechnologies.Count == 0)
            .ToListAsync();

        foreach (var job in jobs)
        {
            var (required, preferred) = JSearchJobService.ExtractTechnologies($"{job.Title} {job.Description}");
            job.RequiredTechnologies = required;
            job.PreferredTechnologies = preferred;
        }

        await _context.SaveChangesAsync();
        return Ok(new { updated = jobs.Count, message = $"Populated technologies for {jobs.Count} jobs" });
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult> SyncJobs([FromServices] IJSearchJobService jSearch)
    {
        if (!User.HasRole("Admin"))
            return Forbid();

        var count = await jSearch.SyncJobsAsync();
        return Ok(new { added = count, message = $"Synced {count} new jobs from JSearch" });
    }

    [HttpGet("{id}/similar")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSimilarJobs(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null) return NotFound();

        var similar = await _context.Jobs
            .Where(j => j.Id != id && j.IsActive && (
                (j.Industry != null && j.Industry == job.Industry) ||
                (j.CompanyName == job.CompanyName) ||
                j.RequiredTechnologies.Any(t => job.RequiredTechnologies.Contains(t))
            ))
            .OrderByDescending(j => j.RequiredTechnologies.Count(t => job.RequiredTechnologies.Contains(t)))
            .Take(6)
            .ToListAsync();

        return Ok(similar);
    }
}
