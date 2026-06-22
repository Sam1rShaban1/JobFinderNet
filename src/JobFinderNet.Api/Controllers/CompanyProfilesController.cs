using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyProfilesController : ControllerBase
{
    private readonly ICompanyProfileService _companyProfileService;

    public CompanyProfilesController(ICompanyProfileService companyProfileService)
    {
        _companyProfileService = companyProfileService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetCompanyProfile(int id)
    {
        var profile = await _companyProfileService.GetByIdAsync(id);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> SearchCompanies([FromQuery] string? q)
    {
        var companies = await _companyProfileService.SearchAsync(q);
        return Ok(companies);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult> GetMyCompany()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Invalid token" });

        var profile = await _companyProfileService.GetMyCompanyAsync(userId);
        if (profile == null)
            return Ok(null);

        return Ok(new
        {
            profile.Id,
            profile.Name,
            profile.LogoUrl,
            profile.Description,
            profile.Website,
            profile.Size,
            profile.Industry,
            profile.IsVerified
        });
    }

    [HttpPost("claim")]
    [Authorize]
    public async Task<ActionResult> ClaimCompany([FromBody] CreateCompanyProfileDto dto)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Invalid token" });

        if (!User.HasRole("Employer"))
            return Forbid();

        if (!User.HasClaim("email_verified", "true"))
            return BadRequest(new { message = "Please verify your email before claiming a company" });

        try
        {
            var company = await _companyProfileService.ClaimCompanyAsync(userId, dto);
            return Ok(new { message = "Company claimed successfully", company.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateCompanyProfile(int id, [FromBody] CreateCompanyProfileDto dto)
    {
        var userId = User.GetUserId()!;
        try
        {
            var company = await _companyProfileService.UpdateCompanyAsync(id, userId, dto);
            if (company == null) return NotFound();
            return Ok(company);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
