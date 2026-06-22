using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public ProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = User.GetUserId()!;
        var profile = await _userProfileService.GetOrCreateProfileAsync(userId);

        return Ok(new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Skills = profile.Skills,
            SeniorityLevel = profile.SeniorityLevel,
            DesiredSalaryMin = profile.DesiredSalaryMin,
            DesiredSalaryMax = profile.DesiredSalaryMax,
            IsOpenToRemote = profile.IsOpenToRemote,
            PreferredLocation = profile.PreferredLocation,
            PreferredJobType = profile.PreferredJobType,
            EmailOnMatch = profile.EmailOnMatch,
            MinimumMatchScore = profile.MinimumMatchScore,
            EmailFrequency = profile.EmailFrequency,
            UpdatedAt = profile.UpdatedAt
        });
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.GetUserId()!;
        var profile = await _userProfileService.UpdateProfileAsync(userId, dto);

        return Ok(new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Skills = profile.Skills,
            SeniorityLevel = profile.SeniorityLevel,
            DesiredSalaryMin = profile.DesiredSalaryMin,
            DesiredSalaryMax = profile.DesiredSalaryMax,
            IsOpenToRemote = profile.IsOpenToRemote,
            PreferredLocation = profile.PreferredLocation,
            PreferredJobType = profile.PreferredJobType,
            EmailOnMatch = profile.EmailOnMatch,
            MinimumMatchScore = profile.MinimumMatchScore,
            EmailFrequency = profile.EmailFrequency,
            UpdatedAt = profile.UpdatedAt
        });
    }

    [HttpGet("matched")]
    public async Task<ActionResult> GetMatchedJobs([FromQuery] int limit = 6)
    {
        var userId = User.GetUserId()!;
        var matches = await _userProfileService.GetMatchedJobsAsync(userId, limit);
        return Ok(matches);
    }

    [HttpGet("matched/detailed")]
    public async Task<ActionResult> GetMatchedJobsDetailed([FromQuery] int limit = 12)
    {
        var userId = User.GetUserId()!;
        var matches = await _userProfileService.GetMatchedJobsDetailedAsync(userId, limit);
        return Ok(matches);
    }

    [HttpGet("skills")]
    public async Task<ActionResult<List<string>>> GetAvailableSkills()
    {
        var skills = await _userProfileService.GetAvailableSkillsAsync();
        return Ok(skills);
    }
}
