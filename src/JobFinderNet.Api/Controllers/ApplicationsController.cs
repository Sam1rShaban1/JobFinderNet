using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost("{jobId}")]
    [Authorize]
    public async Task<ActionResult> Apply(int jobId, [FromBody] ApplyRequest? request = null)
    {
        var userId = User.GetUserId()!;
        if (!User.HasRole("Applicant"))
            return Forbid();

        if (!User.HasClaim("email_verified", "true"))
            return BadRequest(new { message = "Please verify your email before applying" });

        var result = await _applicationService.SubmitApplicationAsync(jobId, userId, request?.CoverLetter);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Application submitted successfully" });
    }

    [HttpGet("my")]
    public async Task<ActionResult> MyApplications()
    {
        var userId = User.GetUserId()!;
        var applications = await _applicationService.GetUserApplicationsAsync(userId);
        return Ok(applications);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<ActionResult> UpdateStatus(int id, UpdateApplicationStatusDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        if (!Enum.TryParse<Core.Models.ApplicationStatus>(dto.Status, true, out var status))
            return BadRequest(new { message = "Invalid status. Use Pending, Screening, Interview, Accepted, or Rejected" });

        var result = await _applicationService.UpdateApplicationStatusAsync(id, status);
        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = $"Application {status.ToString().ToLower()}" });
    }

    [HttpGet("{id}/notes")]
    [Authorize]
    public async Task<ActionResult> GetNotes(int id)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var notes = await _applicationService.GetNotesAsync(id);
        return Ok(notes);
    }

    [HttpPost("{id}/notes")]
    [Authorize]
    public async Task<ActionResult> AddNote(int id, AddNoteDto dto)
    {
        if (!User.HasAnyRole("Employer", "Admin"))
            return Forbid();

        var userId = User.GetUserId()!;
        var note = await _applicationService.AddNoteAsync(id, userId, dto.Content);
        if (note == null) return NotFound();
        return Ok(note);
    }
}
