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
    public async Task<IActionResult> Index(int pageIndex = 1)
    {
        const int PageSize = 10;
        var jobs = await _jobRepository.GetPaginatedJobsAsync(pageIndex, PageSize);
        return View(jobs);
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

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        try
        {
            var job = new Job
            {
                Id = id,
                Title = "Test Job Position",
                JobType = "Full-time",
                CompanyName = "Test Company",
                Location = "Test Location",
                Salary = "$50,000 - $70,000",
                ExperienceRequired = "2+ years",
                Description = "<p>This is a test job description.</p>",
                PostedDate = DateTime.Now
            };

            return View(job);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Details action: {ex.Message}");
            return Content($"Error: {ex.Message}");
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction(nameof(Index));
        
        var jobs = await _jobRepository.SearchJobsAsync(query);
        return View("Index", jobs);
    }
}