using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class ResumeControllerTests
{
    private readonly Mock<IAiService> _mockAiService;
    private readonly ResumeController _controller;

    public ResumeControllerTests()
    {
        _mockAiService = new Mock<IAiService>();
        _controller = new ResumeController(_mockAiService.Object, new Mock<ILogger<ResumeController>>().Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new("sub", "test-user-id"),
            new(ClaimTypes.Role, "Applicant")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task ParseResume_Valid_ReturnsOk()
    {
        var request = new ParseResumeRequest { ResumeText = "My resume text" };
        var parsed = new ParsedResume { Skills = ["C#", "ASP.NET"] };
        _mockAiService.Setup(s => s.ParseResumeAsync(request)).ReturnsAsync(parsed);

        var result = await _controller.ParseResume(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ParsedResume>(okResult.Value);
        Assert.Equal(2, response.Skills.Count);
    }

    [Fact]
    public async Task ParseResume_ArgumentException_ReturnsBadRequest()
    {
        var request = new ParseResumeRequest { ResumeText = "" };
        _mockAiService.Setup(s => s.ParseResumeAsync(request)).ThrowsAsync(new ArgumentException("Invalid text"));

        var result = await _controller.ParseResume(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ParseResume_AsEmployer_ReturnsForbid()
    {
        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "emp1"),
            new(ClaimTypes.Role, "Employer")
        }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.ParseResume(new ParseResumeRequest());

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetRecommendations_Valid_ReturnsOk()
    {
        var request = new ParseResumeRequest { ResumeText = "My resume" };
        var parsed = new ParsedResume { Skills = ["C#"] };
        var recommendations = new List<MatchedJobDto>
        {
            new() { Id = 1, Title = "Dev", CompanyName = "C", Score = 85 }
        };
        _mockAiService.Setup(s => s.ParseResumeAsync(request)).ReturnsAsync(parsed);
        _mockAiService.Setup(s => s.GetRecommendationsAsync("test-user-id", parsed, 10)).ReturnsAsync(recommendations);

        var result = await _controller.GetRecommendations(request, 10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResumeRecommendationDto>(okResult.Value);
        Assert.Single(response.Recommendations);
    }

    [Fact]
    public async Task GetRecommendationsFromSkills_Valid_ReturnsOk()
    {
        var skills = new List<string> { "C#", "React" };
        var recommendations = new List<MatchedJobDto>
        {
            new() { Id = 1, Title = "Dev", CompanyName = "C", Score = 90 }
        };
        _mockAiService.Setup(s => s.GetRecommendationsAsync("test-user-id", It.IsAny<ParsedResume>(), 5))
            .ReturnsAsync(recommendations);

        var result = await _controller.GetRecommendationsFromSkills(skills, 5);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<List<MatchedJobDto>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GenerateCoverLetter_Valid_ReturnsOk()
    {
        var request = new CoverLetterRequest { JobTitle = "Dev", CompanyName = "Corp", JobDescription = "A great job" };
        var response = new CoverLetterResponse { CoverLetter = "Dear Hiring Manager..." };
        _mockAiService.Setup(s => s.GenerateCoverLetterAsync("test-user-id", request)).ReturnsAsync(response);

        var result = await _controller.GenerateCoverLetter(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resp = Assert.IsType<CoverLetterResponse>(okResult.Value);
        Assert.Equal("Dear Hiring Manager...", resp.CoverLetter);
    }

    [Fact]
    public async Task GenerateCoverLetter_AsEmployer_ReturnsForbid()
    {
        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "emp1"),
            new(ClaimTypes.Role, "Employer")
        }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await _controller.GenerateCoverLetter(new CoverLetterRequest());

        Assert.IsType<ForbidResult>(result.Result);
    }
}
