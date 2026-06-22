using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Api.Controllers;

namespace JobFinderNet.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var logger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockUserManager.Object, logger.Object);
    }

    private void SetUser(string userId, string email)
    {
        var claims = new List<Claim>
        {
            new("sub", userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    [Fact]
    public void Controller_CanBeConstructed()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public async Task Me_ExistingUser_ReturnsUserInfo()
    {
        SetUser("test-user-id", "test@example.com");
        var user = new ApplicationUser { Id = "test-user-id", UserName = "test@example.com", Email = "test@example.com" };
        _mockUserManager.Setup(u => u.FindByIdAsync("test-user-id")).ReturnsAsync(user);
        _mockUserManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Employer" });

        var result = await _controller.Me();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("test-user-id", response.UserId);
        Assert.Equal("Employer", response.Role);
    }

    [Fact]
    public async Task Me_NewUser_CreatesAndReturns()
    {
        SetUser("new-user", "new@example.com");
        _mockUserManager.Setup(u => u.FindByIdAsync("new-user")).ReturnsAsync((ApplicationUser?)null);
        _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Applicant" });

        var result = await _controller.Me();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("new-user", response.UserId);
    }

    [Fact]
    public async Task Me_NoSubClaim_ReturnsUnauthorized()
    {
        var identity = new ClaimsIdentity("TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var result = await _controller.Me();

        var unauthResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(unauthResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Me_NewUserCreationFails_ReturnsUnauthorized()
    {
        SetUser("fail-user", "fail@example.com");
        _mockUserManager.Setup(u => u.FindByIdAsync("fail-user")).ReturnsAsync((ApplicationUser?)null);
        _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

        var result = await _controller.Me();

        var unauthResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(unauthResult.Value);
        Assert.False(response.Success);
    }
}
