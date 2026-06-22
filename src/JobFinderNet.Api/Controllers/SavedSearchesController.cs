using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Api.Helpers;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedSearchesController : ControllerBase
{
    private readonly ISavedSearchService _savedSearchService;

    public SavedSearchesController(ISavedSearchService savedSearchService)
    {
        _savedSearchService = savedSearchService;
    }

    [HttpGet]
    public async Task<ActionResult> GetSavedSearches()
    {
        var userId = User.GetUserId()!;
        var searches = await _savedSearchService.GetUserSavedSearchesAsync(userId);
        return Ok(searches);
    }

    [HttpPost]
    public async Task<ActionResult> CreateSavedSearch([FromBody] SavedSearchDto dto)
    {
        var userId = User.GetUserId()!;
        try
        {
            var savedSearch = await _savedSearchService.CreateSavedSearchAsync(userId, dto);
            return Ok(savedSearch);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSavedSearch(int id, [FromBody] SavedSearchDto dto)
    {
        var userId = User.GetUserId()!;
        var savedSearch = await _savedSearchService.UpdateSavedSearchAsync(id, userId, dto);
        if (savedSearch == null) return NotFound();
        return Ok(savedSearch);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSavedSearch(int id)
    {
        var userId = User.GetUserId()!;
        try
        {
            await _savedSearchService.DeleteSavedSearchAsync(id, userId);
            return Ok(new { message = "Saved search deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/run")]
    public async Task<ActionResult> RunSavedSearch(int id)
    {
        var userId = User.GetUserId()!;
        var result = await _savedSearchService.RunSavedSearchAsync(id, userId);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
