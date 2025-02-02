using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Repositories;
using JobFinderNet.Models;
using Microsoft.Extensions.Logging;

namespace JobFinderNet.Controllers;

[Authorize(Roles = "Employer,Admin")]
public class JobsController : Controller
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ILogger<JobsController> _logger;
    
    public JobsController(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        ILogger<JobsController> logger)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _logger = logger;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        int page = 1; // Set the page number
        int pageSize = 10; // Set the number of jobs to display per page
        var jobs = await _jobRepository.GetActiveJobsAsync(page, pageSize);
        return View(jobs);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        _logger.LogInformation("Fetching job details for ID: {Id}", id);
        
        var job = await _jobRepository.GetByIdAsync(id);
        
        _logger.LogInformation("Job found: {JobFound}", job != null);
        
        if (job == null)
        {
            _logger.LogWarning("Job not found for ID: {Id}", id);
            return NotFound();
        }
        
        return View(job);
    }

    [Authorize(Roles = "Employer")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Employer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Job job)
    {
        if (!ModelState.IsValid)
            return View(job);

        job.EmployerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
            throw new InvalidOperationException("User ID not found");
            
        await _jobRepository.CreateJobAsync(job);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Applications(int jobId)
    {
        return View(await _applicationRepository.GetJobApplications(jobId));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction(nameof(Index));
        
        var jobs = await _jobRepository.SearchJobsAsync(query);
        return View("Index", jobs);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Jobs()
    {
        int page = 1; // Set the page number
        int pageSize = 10; // Set the number of jobs to display per page
        var jobs = await _jobRepository.GetActiveJobsAsync(page, pageSize);
        return View(jobs);
    }

    [AllowAnonymous]
    public async Task<IActionResult> LoadMoreJobs(int page = 1)
    {
        int pageSize = 10; // Number of jobs to load per request
        var jobs = await _jobRepository.GetActiveJobsAsync(page, pageSize);
        return Json(jobs);
    }
}