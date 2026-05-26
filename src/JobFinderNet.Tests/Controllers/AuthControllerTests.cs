using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Api.Controllers;
using JobFinderNet.Api.Auth;
using JobFinderNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Tests.Helpers;

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

        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        var confirmation = new Mock<Microsoft.AspNetCore.Identity.IUserConfirmation<ApplicationUser>>();
        var signInManager = new SignInManager<ApplicationUser>(
            _mockUserManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
            schemes.Object,
            confirmation.Object);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"AuthTestDb_{Guid.NewGuid()}")
            .Options;
        var context = new ApplicationDbContext(options);

        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("TestKeyThatIsLongEnoughForHmacSha256!!!");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("Test");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("Test");
        var jwtService = new JwtService(mockConfig.Object);

        _controller = new AuthController(
            _mockUserManager.Object,
            signInManager,
            jwtService,
            context);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var dto = new RegisterDto
        {
            Email = "existing@test.com",
            Password = "Password123",
            Role = "Applicant"
        };

        _mockUserManager.Setup(u => u.FindByEmailAsync("existing@test.com"))
            .ReturnsAsync(new ApplicationUser());

        var result = await _controller.Register(dto);

        var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(actionResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Email already registered", response.Message);
    }
}
