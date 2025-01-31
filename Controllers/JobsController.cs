using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Repositories;
using JobFinderNet.Models;

[Authorize(Roles = "Employer,Admin")]
public class JobsController : Controller
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    
    public JobsController(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await _jobRepository.GetActiveJobsAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Job job)
    {
        if (ModelState.IsValid)
        {
            job.EmployerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                throw new InvalidOperationException("User ID not found");
            await _jobRepository.CreateJobAsync(job);
            return RedirectToAction(nameof(Index));
        }
        return View(job);
    }

    public async Task<IActionResult> Applications(int jobId)
    {
        return View(await _applicationRepository.GetJobApplications(jobId));
    }
}