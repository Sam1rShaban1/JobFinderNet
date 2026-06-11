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
    private readonly ILogger<ResumeController> _logger;

    public ResumeController(IAiService aiService, ILogger<ResumeController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("parse")]
    public async Task<ActionResult<ParsedResume>> ParseResume([FromBody] ParseResumeRequest request)
    {
        if (!User.HasRole("Applicant"))
            return Forbid();

        var userId = User.GetUserId() ?? "unknown";

        try
        {
            var result = await _aiService.ParseResumeAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid resume parse request from user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse resume for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to process resume. Please try again." });
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
            _logger.LogWarning(ex, "Invalid recommendation request from user {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recommendations for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to get recommendations. Please try again." });
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
            _logger.LogError(ex, "Failed to get skill-based recommendations for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to get recommendations. Please try again." });
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
            _logger.LogError(ex, "Failed to generate cover letter for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to generate cover letter. Please try again." });
        }
    }
}
