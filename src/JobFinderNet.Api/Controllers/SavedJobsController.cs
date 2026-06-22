using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedJobsController : ControllerBase
{
    private readonly ISavedJobService _savedJobService;

    public SavedJobsController(ISavedJobService savedJobService)
    {
        _savedJobService = savedJobService;
    }

    [HttpGet]
    public async Task<ActionResult> GetSavedJobs()
    {
        var userId = User.GetUserId()!;
        var saved = await _savedJobService.GetUserSavedJobsAsync(userId);
        return Ok(saved);
    }

    [HttpPost("{jobId}")]
    public async Task<ActionResult> SaveJob(int jobId)
    {
        var userId = User.GetUserId()!;
        try
        {
            await _savedJobService.SaveJobAsync(userId, jobId);
            return Ok(new { message = "Job saved" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{jobId}")]
    public async Task<ActionResult> UnsaveJob(int jobId)
    {
        var userId = User.GetUserId()!;
        try
        {
            await _savedJobService.UnsaveJobAsync(userId, jobId);
            return Ok(new { message = "Job unsaved" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("ids")]
    public async Task<ActionResult> GetSavedJobIds()
    {
        var userId = User.GetUserId()!;
        var ids = await _savedJobService.GetUserSavedJobIdsAsync(userId);
        return Ok(ids);
    }
}
