using JobFinderNet.Models;
using JobFinderNet.Repositories;
using JobFinderNet.Data;
using Microsoft.AspNetCore.Identity;

namespace JobFinderNet.Services;

public class JobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobService(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
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
        
        if (await _applicationRepository.HasUserAppliedToJob(userId, jobId))
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
            Status = ApplicationStatus.Pending,
            AppliedDate = DateTime.UtcNow
        };
        
        var success = await _applicationRepository.AddAsync(application);
        return success 
            ? ApplicationResult.CreateSuccess(application)
            : ApplicationResult.CreateError("Failed to save application");
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

    public async Task<bool> CanUserApplyToJob(ApplicationUser user, int jobId)
    {
        if (user == null) return false;

        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null || !job.IsActive) return false;

        var hasApplied = await _applicationRepository.HasUserAppliedToJob(user.Id, jobId);
        return !hasApplied;
    }

    public async Task<ApplicationResult> CreateApplication(int jobId, string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null) return ApplicationResult.CreateError("Job not found");

        var applicant = await _userManager.FindByIdAsync(userId);
        if (applicant == null) return ApplicationResult.CreateError("User not found");

        var application = new Application
        {
            JobId = jobId,
            Job = job,
            ApplicantId = userId,
            Applicant = applicant,
            Status = ApplicationStatus.Pending
        };
        
        await _applicationRepository.AddAsync(application);
        return ApplicationResult.CreateSuccess(application);
    }
}