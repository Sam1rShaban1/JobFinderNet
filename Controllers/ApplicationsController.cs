using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Services;
using JobFinderNet.Repositories;
using JobFinderNet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Data;

namespace JobFinderNet.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private readonly JobService _jobService;
    private readonly IApplicationRepository _repository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationsController(
        JobService jobService,
        IApplicationRepository repository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _jobService = jobService;
        _repository = repository;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Apply(int jobId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null) return NotFound();

        // Check if user has already applied
        var existingApplication = await _context.Applications
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.ApplicantId == user.Id);

        if (existingApplication != null)
        {
            TempData["Error"] = "You have already applied for this job.";
            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }

               var application = new Application
        {
            JobId = jobId,
            ApplicantId = user.Id,
            AppliedDate = DateTime.UtcNow,
            Status = ApplicationStatus.Pending,
            Job = job,
            Applicant = user
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Your application has been submitted successfully!";
        return RedirectToAction("Details", "Jobs", new { id = jobId });
    }

    [HttpGet]
    public async Task<IActionResult> MyApplications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var applications = await _context.Applications
            .Include(a => a.Job)
            .Where(a => a.ApplicantId == user.Id)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();

        return View(applications);
    }
} 