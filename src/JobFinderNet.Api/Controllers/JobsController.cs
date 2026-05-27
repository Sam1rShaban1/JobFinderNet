using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<JobsController> logger)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
        var employerId = User.FindFirstValue("sub") 
            ?? throw new InvalidOperationException("User ID not found");

        var employer = await _userManager.FindByIdAsync(employerId);
        if (employer == null) return Unauthorized();

        var job = new Job
        {
            Title = dto.Title,
            Description = dto.Description,
            CompanyName = dto.CompanyName,
            Location = dto.Location,
            JobType = dto.JobType,
            Salary = dto.Salary,
            ExperienceRequired = dto.ExperienceRequired,
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
        var employerId = User.FindFirstValue("sub")!;
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
}
