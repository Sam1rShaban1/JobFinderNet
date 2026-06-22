using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var jobs = await _jobService.GetPaginatedJobsAsync(page, pageSize);
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
    public async Task<ActionResult<Core.Models.Job>> GetJob(int id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<Core.Models.Job>());

        var jobs = await _jobService.SearchJobsAsync(query);
        return Ok(jobs);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Core.Models.Job>> CreateJob(CreateJobDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        if (!User.HasClaim("email_verified", "true"))
            return BadRequest(new { message = "Please verify your email before posting jobs" });

        var employerId = User.GetUserId()
            ?? throw new InvalidOperationException("User ID not found");

        try
        {
            var job = await _jobService.CreateJobAsync(dto, employerId);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<Core.Models.Job>> UpdateJob(int id, CreateJobDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var employerId = User.GetUserId()!;
        var job = await _jobService.UpdateJobAsync(id, dto, employerId);
        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteJob(int id)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var employerId = User.GetUserId()!;
        try
        {
            await _jobService.DeleteJobAsync(id, employerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/toggle")]
    [Authorize]
    public async Task<ActionResult> ToggleJobStatus(int id)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var employerId = User.GetUserId()!;
        try
        {
            await _jobService.ToggleJobStatusAsync(id, employerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("employer")]
    [Authorize]
    public async Task<ActionResult> GetEmployerJobs()
    {
        if (!User.HasRole("Employer"))
            return Forbid();

        var employerId = User.GetUserId()!;
        var jobs = await _jobService.GetEmployerJobsAsync(employerId);
        return Ok(jobs);
    }

    [HttpGet("{jobId}/applications")]
    [Authorize]
    public async Task<ActionResult> GetJobApplications(int jobId)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var employerId = User.GetUserId()!;
        try
        {
            var apps = await _jobService.GetJobApplicationsAsync(jobId, employerId);
            return Ok(apps);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("populate-techs")]
    [Authorize]
    public async Task<ActionResult> PopulateTechnologies()
    {
        if (!User.HasRole("Admin"))
            return Forbid();

        await _jobService.PopulateTechnologiesAsync();
        return Ok(new { message = "Technologies populated successfully" });
    }

    [HttpGet("{id}/similar")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSimilarJobs(int id)
    {
        var similar = await _jobService.GetSimilarJobsAsync(id);
        return Ok(similar);
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
}
