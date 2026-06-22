using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class CompanyProfilesControllerTests
{
    private readonly Mock<ICompanyProfileService> _mockService;
    private readonly CompanyProfilesController _controller;

    public CompanyProfilesControllerTests()
    {
        _mockService = new Mock<ICompanyProfileService>();
        _controller = new CompanyProfilesController(_mockService.Object);
        SetUser("test-user-id", "Employer");
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
    public async Task GetCompanyProfile_Existing_ReturnsOk()
    {
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CompanyProfileDto { Id = 1, Name = "Acme" });

        var result = await _controller.GetCompanyProfile(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCompanyProfile_NotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((CompanyProfileDto?)null);

        var result = await _controller.GetCompanyProfile(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SearchCompanies_ReturnsOk()
    {
        _mockService.Setup(s => s.SearchAsync("acme")).ReturnsAsync(new List<CompanySearchResultDto>());

        var result = await _controller.SearchCompanies("acme");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMyCompany_HasCompany_ReturnsOk()
    {
        _mockService.Setup(s => s.GetMyCompanyAsync("test-user-id"))
            .ReturnsAsync(new CompanyProfile { Id = 1, Name = "Acme" });

        var result = await _controller.GetMyCompany();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMyCompany_NoCompany_ReturnsOkNull()
    {
        _mockService.Setup(s => s.GetMyCompanyAsync("test-user-id"))
            .ReturnsAsync((CompanyProfile?)null);

        var result = await _controller.GetMyCompany();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ClaimCompany_Valid_ReturnsOk()
    {
        var dto = new CreateCompanyProfileDto { Name = "NewCo" };
        _mockService.Setup(s => s.ClaimCompanyAsync("test-user-id", dto))
            .ReturnsAsync(new CompanyProfile { Id = 1, Name = "NewCo" });

        var result = await _controller.ClaimCompany(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ClaimCompany_AlreadyClaimed_ReturnsBadRequest()
    {
        var dto = new CreateCompanyProfileDto { Name = "Claimed" };
        _mockService.Setup(s => s.ClaimCompanyAsync("test-user-id", dto))
            .ThrowsAsync(new InvalidOperationException("Company already claimed"));

        var result = await _controller.ClaimCompany(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCompanyProfile_Owner_ReturnsOk()
    {
        var dto = new CreateCompanyProfileDto { Name = "Updated" };
        _mockService.Setup(s => s.UpdateCompanyAsync(1, "test-user-id", dto))
            .ReturnsAsync(new CompanyProfile { Id = 1, Name = "Updated" });

        var result = await _controller.UpdateCompanyProfile(1, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCompanyProfile_NotOwner_ReturnsForbid()
    {
        var dto = new CreateCompanyProfileDto { Name = "X" };
        _mockService.Setup(s => s.UpdateCompanyAsync(1, "test-user-id", dto))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.UpdateCompanyProfile(1, dto);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateCompanyProfile_NotFound_ReturnsNotFound()
    {
        var dto = new CreateCompanyProfileDto { Name = "X" };
        _mockService.Setup(s => s.UpdateCompanyAsync(99, "test-user-id", dto))
            .ReturnsAsync((CompanyProfile?)null);

        var result = await _controller.UpdateCompanyProfile(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }
}
