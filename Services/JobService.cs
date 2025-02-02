using JobFinderNet.Repositories;
using JobFinderNet.Models;
using JobFinderNet.Data;
using ApplicationUser = JobFinderNet.Data.ApplicationUser;
using Microsoft.AspNetCore.Identity;

namespace JobFinderNet.Services;

public class JobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _appRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobService(
        IJobRepository jobRepository,
        IApplicationRepository appRepository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _appRepository = appRepository;
        _context = context;
        _userManager = userManager;
    }
    
    public async Task<ApplicationResult> ApplyForJob(int jobId, string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
            return ApplicationResult.CreateError("Job not found");
        
        if (!job.IsActive)
            return ApplicationResult.CreateError("Job is not active");
        
        if (await _appRepository.HasApplied(userId, jobId))
            return ApplicationResult.CreateError("Already applied");

        var applicant = await _context.Users.FindAsync(userId) as ApplicationUser;
        if (applicant == null)
            return ApplicationResult.CreateError("User not found");
        
        var application = new Application
        {
            JobId = jobId,
            Job = job,
            ApplicantId = userId,
            Applicant = applicant,
            Status = ApplicationStatus.Pending
        };
        
        await _appRepository.AddAsync(application);
        return ApplicationResult.CreateSuccess(application);
    }

    public async Task<bool> ApplyToJob(int jobId, string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null) return false;
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Now we know both job and user are not null
        return await _jobRepository.ApplyForJobAsync(jobId, userId);
    }
}