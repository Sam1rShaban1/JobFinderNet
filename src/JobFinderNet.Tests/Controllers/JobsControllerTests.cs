using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class JobsControllerTests
{
    private readonly Mock<IJobService> _mockJobService;
    private readonly JobsController _controller;

    public JobsControllerTests()
    {
        _mockJobService = new Mock<IJobService>();

        _controller = new JobsController(
            _mockJobService.Object,
            new Mock<ILogger<JobsController>>().Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Role, "Employer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetJobs_ReturnsOkResult()
    {
        var paginatedList = new PaginatedList<Job>(new List<Job>(), 0, 1, 12);
        _mockJobService.Setup(r => r.GetPaginatedJobsAsync(1, 12)).ReturnsAsync(paginatedList);

        var result = await _controller.GetJobs();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetJob_ExistingId_ReturnsJob()
    {
        var employer = new ApplicationUser { Id = "emp1", UserName = "emp@test.com", Email = "emp@test.com" };
        var job = new Job
        {
            Id = 1,
            Title = "Test Job",
            Description = "Desc",
            CompanyName = "Test Corp",
            Location = "Remote",
            JobType = "Full-time",
            Salary = "$80k",
            ExperienceRequired = "Entry",
            EmployerId = employer.Id,
            Employer = employer,
            IsActive = true,
            PostedDate = DateTime.UtcNow
        };

        _mockJobService.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var result = await _controller.GetJob(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedJob = Assert.IsType<Job>(okResult.Value);
        Assert.Equal("Test Job", returnedJob.Title);
    }

    [Fact]
    public async Task GetJob_NonExistentId_ReturnsNotFound()
    {
        _mockJobService.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Job?)null);

        var result = await _controller.GetJob(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Search_WithQuery_ReturnsResults()
    {
        var jobs = new List<Job>
        {
            new() {
                Id = 1, Title = "Software Engineer", Description = "Desc",
                CompanyName = "Tech Corp", Location = "Remote", JobType = "Full-time",
                Salary = "$100k", ExperienceRequired = "Senior",
                EmployerId = "emp1", Employer = null!,
                IsActive = true, PostedDate = DateTime.UtcNow
            }
        };

        _mockJobService.Setup(r => r.SearchJobsAsync("Engineer")).ReturnsAsync(jobs);

        var result = await _controller.Search("Engineer");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedJobs = Assert.IsType<List<Job>>(okResult.Value);
        Assert.Single(returnedJobs);
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsEmptyList()
    {
        var result = await _controller.Search("");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var jobs = Assert.IsType<List<Job>>(okResult.Value);
        Assert.Empty(jobs);
    }
}
