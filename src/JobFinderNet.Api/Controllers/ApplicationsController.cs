using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Api.Helpers;

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
    [Authorize]
    public async Task<ActionResult> Apply(int jobId, [FromBody] ApplyRequest? request = null)
    {
        var userId = User.GetUserId()!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (!User.HasRole("Applicant"))
            return Forbid();

        if (!User.HasClaim("email_verified", "true") && !user.EmailConfirmed)
            return BadRequest(new { message = "Please verify your email before applying" });

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
            AppliedDate = DateTime.UtcNow,
            CoverLetter = request?.CoverLetter
        };

        _context.Entry(job).State = EntityState.Unchanged;
        _context.Entry(user).State = EntityState.Unchanged;
        var success = await _applicationRepository.AddAsync(application);
        if (!success)
            return BadRequest(new { message = "Failed to submit application" });

        // Create in-app notification for employer
        _context.Notifications.Add(new AppNotification
        {
            UserId = job.EmployerId,
            Title = "New Application",
            Message = $"{user.UserName ?? user.Email} applied to {job.Title}",
            Link = $"/jobs/{job.Id}",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(new { message = "Application submitted successfully" });
    }

    [HttpGet("my")]
    public async Task<ActionResult> MyApplications()
    {
        var userId = User.GetUserId()!;
        var applications = await _applicationRepository.GetUserApplicationsAsync(userId);
        return Ok(applications);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<ActionResult> UpdateStatus(int id, UpdateApplicationStatusDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var application = await _context.Applications.FindAsync(id);
        if (application == null) return NotFound();

        if (!Enum.TryParse<ApplicationStatus>(dto.Status, true, out var status))
            return BadRequest(new { message = "Invalid status. Use Pending, Screening, Interview, Accepted, or Rejected" });

        application.Status = status;
        await _context.SaveChangesAsync();

        // Create in-app notification for applicant
        var statusLabel = status switch
        {
            ApplicationStatus.Accepted => "accepted",
            ApplicationStatus.Rejected => "not selected",
            ApplicationStatus.Screening => "moved to screening",
            ApplicationStatus.Interview => "moved to interview",
            _ => status.ToString().ToLower()
        };
        _context.Notifications.Add(new AppNotification
        {
            UserId = application.ApplicantId,
            Title = "Application Update",
            Message = $"Your application has been {statusLabel}",
            Link = "/my-applications",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Application {status.ToString().ToLower()}" });
    }

    [HttpGet("{id}/notes")]
    [Authorize]
    public async Task<ActionResult> GetNotes(int id)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var notes = await _context.ApplicationNotes
            .Where(n => n.ApplicationId == id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPost("{id}/notes")]
    [Authorize]
    public async Task<ActionResult> AddNote(int id, AddNoteDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var application = await _context.Applications.FindAsync(id);
        if (application == null) return NotFound();

        var userId = User.GetUserId()!;

        var note = new ApplicationNote
        {
            ApplicationId = id,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApplicationNotes.Add(note);
        await _context.SaveChangesAsync();

        return Ok(note);
    }
}
