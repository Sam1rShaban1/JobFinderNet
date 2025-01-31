using JobFinderNet.Repositories;
using JobFinderNet.Models;

namespace JobFinderNet.Services;

public class JobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _appRepository;

    public JobService(
        IJobRepository jobRepository,
        IApplicationRepository appRepository)
    {
        _jobRepository = jobRepository;
        _appRepository = appRepository;
    }
    
    public async Task<ApplicationResult> ApplyForJob(int jobId, string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (!job.IsActive)
            return ApplicationResult.CreateError("Job is not active");
        
        if (await _appRepository.HasApplied(userId, jobId))
            return ApplicationResult.CreateError("Already applied");
        
        var application = new JobApplication
        {
            ApplicantId = userId,
            JobId = jobId,
            Status = ApplicationStatus.Pending
        };
        
             await _appRepository.AddAsync(application);
        return ApplicationResult.CreateSuccess(application);
    }
}