using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationsController(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("{jobId}")]
    [Authorize(Roles = "Applicant")]
    public async Task<ActionResult> Apply(int jobId)
    {
        var userId = User.FindFirstValue("sub")!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null || !job.IsActive)
            return NotFound(new { message = "Job not found or inactive" });

        var hasApplied = await _applicationRepository.HasUserAppliedToJob(userId, jobId);
        if (hasApplied)
            return BadRequest(new { message = "Already applied to this job" });

        var application = new Application
        {
            JobId = jobId,
            Job = job,
            ApplicantId = userId,
            Applicant = user,
            Status = ApplicationStatus.Pending,
            AppliedDate = DateTime.UtcNow
        };

        var success = await _applicationRepository.AddAsync(application);
        if (!success)
            return BadRequest(new { message = "Failed to submit application" });

        return Ok(new { message = "Application submitted successfully" });
    }

    [HttpGet("my")]
    public async Task<ActionResult> MyApplications()
    {
        var userId = User.FindFirstValue("sub")!;
        var applications = await _applicationRepository.GetUserApplicationsAsync(userId);
        return Ok(applications);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult> UpdateStatus(int id, UpdateApplicationStatusDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null) return NotFound();

        if (!Enum.TryParse<ApplicationStatus>(dto.Status, true, out var status))
            return BadRequest(new { message = "Invalid status. Use Pending, Accepted, or Rejected" });

        application.Status = status;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Application {status.ToString().ToLower()}" });
    }
}
