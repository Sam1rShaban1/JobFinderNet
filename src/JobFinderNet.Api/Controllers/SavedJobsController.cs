using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedJobsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SavedJobsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetSavedJobs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var saved = await _context.SavedJobs
            .Include(s => s.Job)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SavedDate)
            .ToListAsync();

        return Ok(saved);
    }

    [HttpPost("{jobId}")]
    public async Task<ActionResult> SaveJob(int jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null) return NotFound();

        var existing = await _context.SavedJobs
            .FirstOrDefaultAsync(s => s.UserId == userId && s.JobId == jobId);
        if (existing != null)
            return Ok(new { message = "Already saved" });

        var saved = new SavedJob
        {
            UserId = userId,
            JobId = jobId,
            SavedDate = DateTime.UtcNow
        };

        _context.SavedJobs.Add(saved);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Job saved" });
    }

    [HttpDelete("{jobId}")]
    public async Task<ActionResult> UnsaveJob(int jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var saved = await _context.SavedJobs
            .FirstOrDefaultAsync(s => s.UserId == userId && s.JobId == jobId);

        if (saved == null) return NotFound();

        _context.SavedJobs.Remove(saved);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Job unsaved" });
    }

    [HttpGet("ids")]
    public async Task<ActionResult> GetSavedJobIds()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var ids = await _context.SavedJobs
            .Where(s => s.UserId == userId)
            .Select(s => s.JobId)
            .ToListAsync();

        return Ok(ids);
    }
}
