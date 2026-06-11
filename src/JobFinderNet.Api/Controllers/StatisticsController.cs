using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Api.Helpers;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(ApplicationDbContext context, IStatisticsService statisticsService)
    {
        _context = context;
        _statisticsService = statisticsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetStatistics()
    {
        var totalJobs = await _context.Jobs.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalApplications = await _context.Applications.CountAsync();
        var jobsWithTech = await _context.Jobs.CountAsync(j =>
            j.RequiredTechnologies.Count > 0 || j.PreferredTechnologies.Count > 0);

        var allJobs = await _context.Jobs
            .Select(j => new { j.RequiredTechnologies, j.PreferredTechnologies })
            .ToListAsync();

        var allTech = allJobs
            .SelectMany(j => j.RequiredTechnologies)
            .Concat(allJobs.SelectMany(j => j.PreferredTechnologies))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .Count();

        var jobsByType = await _context.Jobs
            .GroupBy(j => j.JobType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        return Ok(new
        {
            totalJobs,
            totalUsers,
            totalApplications,
            jobsWithTech,
            totalTechnologies = allTech,
            jobsByType
        });
    }

    [HttpGet("employer")]
    [Authorize]
    public async Task<ActionResult> GetEmployerDashboard()
    {
        if (!User.HasRole("Employer"))
            return Forbid();

        var employerId = User.GetUserId();
        if (string.IsNullOrEmpty(employerId))
            return Unauthorized();

        var dashboard = await _statisticsService.GetEmployerDashboardAsync(employerId);
        return Ok(dashboard);
    }
}
