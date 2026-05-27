using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<ApplicationUser> userManager, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AuthResponseDto>> Me()
    {
        var userId = User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");

        if (userId == null)
        {
            _logger.LogWarning("Auth/me: no sub claim in JWT");
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Invalid token" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            var email = User.FindFirstValue("email")
                ?? User.FindFirstValue(ClaimTypes.Email)
                ?? $"{userId}@clerk.dev";

            try
            {
                user = new ApplicationUser
                {
                    Id = userId,
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Applicant");
                    _logger.LogInformation("Auth/me: created user {UserId} with email {Email}", userId, email);
                }
                else
                {
                    _logger.LogWarning("Auth/me: failed to create user {UserId}: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Unauthorized(new AuthResponseDto { Success = false, Message = "User creation failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Auth/me: DB error creating user {UserId}", userId);
                return Unauthorized(new AuthResponseDto { Success = false, Message = "Database error" });
            }
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AuthResponseDto
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            Role = roles.FirstOrDefault() ?? "Applicant"
        });
    }
}
