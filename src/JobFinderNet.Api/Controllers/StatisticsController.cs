using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetStatistics()
    {
        var totalJobs = await _context.Jobs.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalApplications = await _context.Applications.CountAsync();
        var jobsWithTech = await _context.Jobs.CountAsync(j =>
            j.RequiredTechnologies.Count > 0 || j.PreferredTechnologies.Count > 0);

        return Ok(new
        {
            totalJobs,
            totalUsers,
            totalApplications,
            jobsWithTech
        });
    }
}
