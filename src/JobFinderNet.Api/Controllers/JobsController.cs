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
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult<Job>> CreateJob(CreateJobDto dto)
    {
        var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new InvalidOperationException("User ID not found");

        var employer = await _userManager.FindByIdAsync(employerId);
        if (employer == null) return Unauthorized();

        var job = new Job
        {
            Title = dto.Title,
            Description = dto.Description,
            CompanyName = dto.CompanyName,
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
            Industry = dto.Industry,
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult> DeleteJob(int id)
    {
        await _jobRepository.DeleteJobAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/toggle")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult> ToggleJobStatus(int id)
    {
        await _jobRepository.ToggleJobStatusAsync(id);
        return NoContent();
    }

    [HttpGet("employer")]
    [Authorize(Roles = "Employer")]
    public async Task<ActionResult> GetEmployerJobs()
    {
        var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var jobs = await _jobRepository.GetEmployerJobsAsync(employerId);
        return Ok(jobs);
    }

    [HttpGet("{jobId}/applications")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult> GetJobApplications(int jobId)
    {
        var apps = await _applicationRepository.GetJobApplications(jobId);
        return Ok(apps);
    }

    [HttpPost("populate-techs")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> PopulateTechnologies()
    {
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SyncJobs([FromServices] IJSearchJobService jSearch)
    {
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
