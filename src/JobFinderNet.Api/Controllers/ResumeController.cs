using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResumeController : ControllerBase
{
    private readonly IAiService _aiService;

    public ResumeController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("parse")]
    public async Task<ActionResult<ParsedResume>> ParseResume([FromBody] ParseResumeRequest request)
    {
        if (!User.HasRole("Applicant"))
            return Forbid();

        try
        {
            var result = await _aiService.ParseResumeAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to parse resume: {ex.Message}" });
        }
    }

    [HttpPost("recommendations")]
    public async Task<ActionResult<ResumeRecommendationDto>> GetRecommendations([FromBody] ParseResumeRequest request, [FromQuery] int limit = 10)
    {
        if (!User.HasRole("Applicant"))
            return Forbid();

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var parsed = await _aiService.ParseResumeAsync(request);
            var recommendations = await _aiService.GetRecommendationsAsync(userId, parsed, limit);

            return Ok(new ResumeRecommendationDto
            {
                ParsedResume = parsed,
                Recommendations = recommendations
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to get recommendations: {ex.Message}" });
        }
    }

    [HttpPost("recommendations/from-skills")]
    public async Task<ActionResult<List<MatchedJobDto>>> GetRecommendationsFromSkills([FromBody] List<string> skills, [FromQuery] int limit = 10)
    {
        if (!User.HasRole("Applicant"))
            return Forbid();

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var parsed = new ParsedResume { Skills = skills };
            var recommendations = await _aiService.GetRecommendationsAsync(userId, parsed, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to get recommendations: {ex.Message}" });
        }
    }

    [HttpPost("cover-letter")]
    public async Task<ActionResult<CoverLetterResponse>> GenerateCoverLetter([FromBody] CoverLetterRequest request)
    {
        if (!User.HasRole("Applicant"))
            return Forbid();

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var result = await _aiService.GenerateCoverLetterAsync(userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to generate cover letter: {ex.Message}" });
        }
    }
}
