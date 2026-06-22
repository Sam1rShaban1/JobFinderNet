using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetStatistics()
    {
        var stats = await _statisticsService.GetPublicStatisticsAsync();
        return Ok(stats);
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
