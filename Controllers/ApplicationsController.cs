using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Services;
using JobFinderNet.Repositories;
using JobFinderNet.Models;

namespace JobFinderNet.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private readonly JobService _jobService;
    private readonly IApplicationRepository _repository;

    public ApplicationsController(
        JobService jobService,
        IApplicationRepository repository)
    {
        _jobService = jobService;
        _repository = repository;
    }

    public async Task<IActionResult> MyApplications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
            throw new InvalidOperationException("User ID not found");
        var applications = await _repository.GetUserApplications(userId);
        return View(applications);
    }

    [HttpPost]
    public async Task<IActionResult> Apply(int jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
            throw new InvalidOperationException("User ID not found");
        var result = await _jobService.ApplyForJob(jobId, userId);
        
        if (!result.Success)
            return BadRequest(result.Error);
            
        return RedirectToAction(nameof(MyApplications));
    }
} 