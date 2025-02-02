using JobFinderNet.Repositories;
using JobFinderNet.Models;
using JobFinderNet.Data;
using ApplicationUser = JobFinderNet.Data.ApplicationUser;

namespace JobFinderNet.Services;

public class JobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _appRepository;
    private readonly ApplicationDbContext _context;

    public JobService(
        IJobRepository jobRepository,
        IApplicationRepository appRepository,
        ApplicationDbContext context)
    {
        _jobRepository = jobRepository;
        _appRepository = appRepository;
        _context = context;
    }
    
    public async Task<ApplicationResult> ApplyForJob(int jobId, string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (!job.IsActive)
            return ApplicationResult.CreateError("Job is not active");
        
        if (await _appRepository.HasApplied(userId, jobId))
            return ApplicationResult.CreateError("Already applied");

        var applicant = await _context.Users.FindAsync(userId);
        if (applicant == null)
            return ApplicationResult.CreateError("User not found");
        
        var application = new JobApplication
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
}