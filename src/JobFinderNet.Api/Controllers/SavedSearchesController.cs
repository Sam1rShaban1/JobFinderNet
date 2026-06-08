using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedSearchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SavedSearchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetSavedSearches()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var searches = await _context.SavedSearches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(searches);
    }

    [HttpPost]
    public async Task<ActionResult> CreateSavedSearch([FromBody] SavedSearchDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            return BadRequest(new { message = "Create a profile first" });

        var filtersJson = JsonSerializer.Serialize(new
        {
            search = dto.Search,
            location = dto.Location,
            jobType = dto.JobType,
            salaryMin = dto.SalaryMin,
            salaryMax = dto.SalaryMax,
            isRemote = dto.IsRemote,
            seniority = dto.Seniority,
            tech = dto.Tech
        });

        var savedSearch = new SavedSearch
        {
            UserId = userId,
            Name = dto.Name,
            FiltersJson = filtersJson,
            EmailFrequency = dto.EmailFrequency,
            CreatedAt = DateTime.UtcNow
        };

        _context.SavedSearches.Add(savedSearch);
        await _context.SaveChangesAsync();

        return Ok(savedSearch);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSavedSearch(int id, [FromBody] SavedSearchDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var savedSearch = await _context.SavedSearches
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (savedSearch == null) return NotFound();

        var filtersJson = JsonSerializer.Serialize(new
        {
            search = dto.Search,
            location = dto.Location,
            jobType = dto.JobType,
            salaryMin = dto.SalaryMin,
            salaryMax = dto.SalaryMax,
            isRemote = dto.IsRemote,
            seniority = dto.Seniority,
            tech = dto.Tech
        });

        savedSearch.Name = dto.Name;
        savedSearch.FiltersJson = filtersJson;
        savedSearch.EmailFrequency = dto.EmailFrequency;

        await _context.SaveChangesAsync();
        return Ok(savedSearch);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSavedSearch(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var savedSearch = await _context.SavedSearches
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (savedSearch == null) return NotFound();

        _context.SavedSearches.Remove(savedSearch);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Saved search deleted" });
    }

    [HttpPost("{id}/run")]
    public async Task<ActionResult> RunSavedSearch(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var savedSearch = await _context.SavedSearches
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (savedSearch == null) return NotFound();

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return BadRequest(new { message = "Profile not found" });

        var matchingService = HttpContext.RequestServices.GetRequiredService<Core.Interfaces.Services.IMatchingService>();
        var matches = await matchingService.GetTopMatches(profile, 10);

        savedSearch.LastRunAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            searchName = savedSearch.Name,
            matchCount = matches.Count,
            matches = matches.Select(m => new
            {
                m.Job.Id,
                m.Job.Title,
                m.Job.CompanyName,
                m.Job.Location,
                Score = m.Score
            })
        });
    }
}
