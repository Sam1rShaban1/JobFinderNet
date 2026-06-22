using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Tests.Controllers;

public class ApplicationsControllerTests
{
    private readonly Mock<IApplicationService> _mockService;
    private readonly ApplicationsController _controller;

    public ApplicationsControllerTests()
    {
        _mockService = new Mock<IApplicationService>();
        _controller = new ApplicationsController(_mockService.Object);
        SetUser("test-user-id", "Applicant");
    }

    private void SetUser(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role),
            new("email_verified", "true")
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
        };
    }

    [Fact]
    public async Task Apply_ValidRequest_ReturnsOk()
    {
        _mockService.Setup(s => s.SubmitApplicationAsync(1, "test-user-id", null))
            .ReturnsAsync(ApplicationResult.CreateSuccess(new Application { Id = 1, JobId = 1, ApplicantId = "test-user-id", Status = ApplicationStatus.Pending, AppliedDate = DateTime.UtcNow, Job = null!, Applicant = null! }));

        var result = await _controller.Apply(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Apply_FailedResult_ReturnsBadRequest()
    {
        _mockService.Setup(s => s.SubmitApplicationAsync(1, "test-user-id", null))
            .ReturnsAsync(ApplicationResult.CreateError("Already applied"));

        var result = await _controller.Apply(1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task MyApplications_ReturnsOk()
    {
        _mockService.Setup(s => s.GetUserApplicationsAsync("test-user-id"))
            .ReturnsAsync(new List<Application>());

        var result = await _controller.MyApplications();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ValidStatus_ReturnsOk()
    {
        SetUser("emp1", "Employer");
        _mockService.Setup(s => s.UpdateApplicationStatusAsync(1, ApplicationStatus.Accepted))
            .ReturnsAsync(ApplicationResult.CreateSuccess(new Application { Id = 1, JobId = 1, ApplicantId = "a1", Status = ApplicationStatus.Accepted, AppliedDate = DateTime.UtcNow, Job = null!, Applicant = null! }));

        var result = await _controller.UpdateStatus(1, new UpdateApplicationStatusDto { Status = "Accepted" });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatus_ReturnsBadRequest()
    {
        SetUser("emp1", "Employer");

        var result = await _controller.UpdateStatus(1, new UpdateApplicationStatusDto { Status = "InvalidStatus" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_Applicant_ReturnsForbid()
    {
        SetUser("test-user-id", "Applicant");

        var result = await _controller.UpdateStatus(1, new UpdateApplicationStatusDto { Status = "Accepted" });

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetNotes_Employer_ReturnsOk()
    {
        SetUser("emp1", "Employer");
        _mockService.Setup(s => s.GetNotesAsync(1)).ReturnsAsync(new List<ApplicationNote>());

        var result = await _controller.GetNotes(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task AddNote_Employer_ReturnsOk()
    {
        SetUser("emp1", "Employer");
        var note = new ApplicationNote { Id = 1, ApplicationId = 1, UserId = "emp1", Content = "Test note", CreatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.AddNoteAsync(1, "emp1", "Test note")).ReturnsAsync(note);

        var result = await _controller.AddNote(1, new AddNoteDto { Content = "Test note" });

        Assert.IsType<OkObjectResult>(result);
    }
}
