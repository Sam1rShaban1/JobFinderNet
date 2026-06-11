using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyProfilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CompanyProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetCompanyProfile(int id)
    {
        var profile = await _context.CompanyProfiles
            .Include(c => c.Jobs.Where(j => j.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (profile == null) return NotFound();

        return Ok(new
        {
            profile.Id,
            profile.Name,
            profile.LogoUrl,
            profile.Description,
            profile.Website,
            profile.Size,
            profile.Industry,
            profile.IsVerified,
            OpenRoles = profile.Jobs.Count
        });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> SearchCompanies([FromQuery] string? q)
    {
        var query = _context.CompanyProfiles.AsQueryable();

        if (!string.IsNullOrEmpty(q))
        {
            query = query.Where(c => c.Name.Contains(q));
        }

        var companies = await query
            .OrderBy(c => c.Name)
            .Take(20)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.LogoUrl,
                c.Industry,
                OpenRoles = c.Jobs.Count(j => j.IsActive)
            })
            .ToListAsync();

        return Ok(companies);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult> GetMyCompany()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? string.Empty;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Invalid token" });

        var profile = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.ClaimedByUserId == userId);

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

        if (!User.HasRole("Employer"))
            return Forbid();

        var existing = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.Name == dto.Name);

        if (existing != null)
        {
            if (existing.ClaimedByUserId != null)
                return BadRequest(new { message = "Company already claimed" });

            existing.ClaimedByUserId = userId;
            existing.LogoUrl = dto.LogoUrl ?? existing.LogoUrl;
            existing.Description = dto.Description ?? existing.Description;
            existing.Website = dto.Website ?? existing.Website;
            existing.Size = dto.Size ?? existing.Size;
            existing.Industry = dto.Industry ?? existing.Industry;
            existing.FoundedYear = dto.FoundedYear ?? existing.FoundedYear;
            existing.Culture = dto.Culture ?? existing.Culture;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Company claimed successfully", existing.Id });
        }

        var company = new CompanyProfile
        {
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            Description = dto.Description,
            Website = dto.Website,
            Size = dto.Size,
            Industry = dto.Industry,
            FoundedYear = dto.FoundedYear,
            Culture = dto.Culture,
            ClaimedByUserId = userId,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CompanyProfiles.Add(company);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Company created and claimed", company.Id });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateCompanyProfile(int id, [FromBody] CreateCompanyProfileDto dto)
    {
        var userId = User.GetUserId()!;
        var company = await _context.CompanyProfiles.FindAsync(id);

        if (company == null) return NotFound();
        if (company.ClaimedByUserId != userId)
            return Forbid();

        company.LogoUrl = dto.LogoUrl ?? company.LogoUrl;
        company.Description = dto.Description ?? company.Description;
        company.Website = dto.Website ?? company.Website;
        company.Size = dto.Size ?? company.Size;
        company.Industry = dto.Industry ?? company.Industry;
        company.FoundedYear = dto.FoundedYear ?? company.FoundedYear;
        company.Culture = dto.Culture ?? company.Culture;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(company);
    }
}
