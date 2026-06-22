using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
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
            new(ClaimTypes.Role, "Employer"),
            new("email_verified", "true")
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

    [Fact]
    public async Task CreateJob_Valid_ReturnsCreated()
    {
        var dto = new CreateJobDto
        {
            Title = "New Job",
            Description = "Desc",
            CompanyName = "Corp",
            Location = "Remote",
            JobType = "Full-time",
            Salary = "$80k",
            ExperienceRequired = "Entry"
        };
        var job = new Job
        {
            Id = 1, Title = "New Job", Description = "Desc", CompanyName = "Corp",
            Location = "Remote", JobType = "Full-time", Salary = "$80k",
            ExperienceRequired = "Entry", EmployerId = "test-user-id", IsActive = true,
            PostedDate = DateTime.UtcNow
        };
        _mockJobService.Setup(s => s.CreateJobAsync(dto, "test-user-id")).ReturnsAsync(job);

        var result = await _controller.CreateJob(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetJob", createdResult.ActionName);
    }

    [Fact]
    public async Task CreateJob_NoEmailVerification_ReturnsBadRequest()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Role, "Employer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.CreateJob(new CreateJobDto());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateJob_AsApplicant_ReturnsForbid()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Role, "Applicant"),
            new("email_verified", "true")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.CreateJob(new CreateJobDto());

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task UpdateJob_Valid_ReturnsOk()
    {
        var dto = new CreateJobDto
        {
            Title = "Updated", Description = "Desc", CompanyName = "C",
            Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry"
        };
        _mockJobService.Setup(s => s.UpdateJobAsync(1, dto, "test-user-id"))
            .ReturnsAsync(new Job { Id = 1, Title = "Updated", Description = "Desc", CompanyName = "C", Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry", EmployerId = "test-user-id", IsActive = true, PostedDate = DateTime.UtcNow });

        var result = await _controller.UpdateJob(1, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateJob_NotFound_ReturnsNotFound()
    {
        var dto = new CreateJobDto
        {
            Title = "Updated", Description = "Desc", CompanyName = "C",
            Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry"
        };
        _mockJobService.Setup(s => s.UpdateJobAsync(1, dto, "test-user-id"))
            .ReturnsAsync((Job?)null);

        var result = await _controller.UpdateJob(1, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateJob_AsApplicant_ReturnsForbid()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Role, "Applicant")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.UpdateJob(1, new CreateJobDto());

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task DeleteJob_Valid_ReturnsNoContent()
    {
        _mockJobService.Setup(s => s.DeleteJobAsync(1, "test-user-id")).Returns(Task.CompletedTask);

        var result = await _controller.DeleteJob(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteJob_NotFound_ReturnsNotFound()
    {
        _mockJobService.Setup(s => s.DeleteJobAsync(1, "test-user-id"))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.DeleteJob(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteJob_AsApplicant_ReturnsForbid()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Role, "Applicant")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.DeleteJob(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ToggleJobStatus_Valid_ReturnsNoContent()
    {
        _mockJobService.Setup(s => s.ToggleJobStatusAsync(1, "test-user-id")).Returns(Task.CompletedTask);

        var result = await _controller.ToggleJobStatus(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ToggleJobStatus_NotFound_ReturnsNotFound()
    {
        _mockJobService.Setup(s => s.ToggleJobStatusAsync(1, "test-user-id"))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.ToggleJobStatus(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetEmployerJobs_ReturnsOk()
    {
        var jobs = new List<Job>
        {
            new() { Id = 1, Title = "Job1", Description = "D", CompanyName = "C", Location = "L",
                JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry",
                EmployerId = "test-user-id", IsActive = true, PostedDate = DateTime.UtcNow }
        };
        _mockJobService.Setup(s => s.GetEmployerJobsAsync("test-user-id")).ReturnsAsync(jobs);

        var result = await _controller.GetEmployerJobs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Job>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetEmployerJobs_AsApplicant_ReturnsForbid()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Role, "Applicant")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.GetEmployerJobs();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetJobApplications_Valid_ReturnsOk()
    {
        var apps = new List<Application>
        {
            new() { Id = 1, JobId = 1, ApplicantId = "a1", Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow, Job = null!, Applicant = null! }
        };
        _mockJobService.Setup(s => s.GetJobApplicationsAsync(1, "test-user-id")).ReturnsAsync(apps);

        var result = await _controller.GetJobApplications(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Application>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetJobApplications_NotFound_ReturnsNotFound()
    {
        _mockJobService.Setup(s => s.GetJobApplicationsAsync(1, "test-user-id"))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetJobApplications(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetSimilarJobs_ReturnsOk()
    {
        var jobs = new List<Job>
        {
            new() { Id = 2, Title = "Similar", Description = "D", CompanyName = "C",
                Location = "L", JobType = "FT", Salary = "$80k", ExperienceRequired = "Entry",
                EmployerId = "emp1", IsActive = true, PostedDate = DateTime.UtcNow }
        };
        _mockJobService.Setup(s => s.GetSimilarJobsAsync(1)).ReturnsAsync(jobs);

        var result = await _controller.GetSimilarJobs(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<List<Job>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task PopulateTechnologies_AsAdmin_ReturnsOk()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "admin1"),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        _mockJobService.Setup(s => s.PopulateTechnologiesAsync()).Returns(Task.CompletedTask);

        var result = await _controller.PopulateTechnologies();

        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task PopulateTechnologies_AsEmployer_ReturnsForbid()
    {
        var result = await _controller.PopulateTechnologies();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task SyncJobs_AsAdmin_ReturnsOk()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "admin1"),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        var mockJSearch = new Mock<IJSearchJobService>();
        mockJSearch.Setup(s => s.SyncJobsAsync()).ReturnsAsync(5);

        var result = await _controller.SyncJobs(mockJSearch.Object);

        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SyncJobs_AsEmployer_ReturnsForbid()
    {
        var mockJSearch = new Mock<IJSearchJobService>();

        var result = await _controller.SyncJobs(mockJSearch.Object);

        Assert.IsType<ForbidResult>(result);
    }
}
