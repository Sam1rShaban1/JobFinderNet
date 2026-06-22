using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class StatisticsControllerTests
{
    private readonly Mock<IStatisticsService> _mockService;
    private readonly StatisticsController _controller;

    public StatisticsControllerTests()
    {
        _mockService = new Mock<IStatisticsService>();
        _controller = new StatisticsController(_mockService.Object);
    }

    private void SetEmployer(string userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, "Employer")
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
        };
    }

    [Fact]
    public async Task GetStatistics_ReturnsOk()
    {
        _mockService.Setup(s => s.GetPublicStatisticsAsync()).ReturnsAsync(new PublicStatisticsDto
        {
            TotalJobs = 10,
            TotalUsers = 50,
            TotalApplications = 100,
            JobsWithTech = 8,
            TotalTechnologies = 20,
            JobsByType = new Dictionary<string, int> { { "Full-time", 7 }, { "Part-time", 3 } }
        });

        var result = await _controller.GetStatistics();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetEmployerDashboard_Employer_ReturnsOk()
    {
        SetEmployer("emp1");
        _mockService.Setup(s => s.GetEmployerDashboardAsync("emp1")).ReturnsAsync(new EmployerDashboardDto
        {
            TotalJobs = 5,
            ActiveJobs = 3,
            TotalApplications = 20
        });

        var result = await _controller.GetEmployerDashboard();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetEmployerDashboard_Applicant_ReturnsForbid()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "app1"),
            new(ClaimTypes.Role, "Applicant")
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
        };

        var result = await _controller.GetEmployerDashboard();

        Assert.IsType<ForbidResult>(result);
    }
}
