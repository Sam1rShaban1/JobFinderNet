using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ApplicationService(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        INotificationService notificationService)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<ApplicationResult> SubmitApplicationAsync(int jobId, string userId, string? coverLetter = null)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
            return ApplicationResult.CreateError("Job not found");

        if (!job.IsActive)
            return ApplicationResult.CreateError("Job is no longer accepting applications");

        if (job.EmployerId == userId)
            return ApplicationResult.CreateError("Employers cannot apply to their own jobs");

        if (await _applicationRepository.HasUserAppliedToJob(userId, jobId))
            return ApplicationResult.CreateError("You have already applied to this job");

        var applicant = await _userManager.FindByIdAsync(userId);
        if (applicant == null)
            return ApplicationResult.CreateError("Applicant not found");

        var application = new Application
        {
            JobId = jobId,
            Job = job,
            ApplicantId = userId,
            Applicant = applicant,
            CoverLetter = coverLetter,
            Status = ApplicationStatus.Pending,
            AppliedDate = DateTime.UtcNow
        };

        var saved = await _applicationRepository.AddAsync(application);
        if (!saved)
            return ApplicationResult.CreateError("Failed to submit application");

        var applicantName = applicant.UserName ?? applicant.Email ?? "Applicant";
        await _notificationService.SendApplicationSubmittedAsync(
            applicant.Email ?? "", applicantName, job.Title, job.CompanyName);

        if (job.Employer != null)
        {
            var employerName = job.Employer.CompanyName ?? job.Employer.UserName ?? "Employer";
            await _notificationService.SendNewApplicationToEmployerAsync(
                job.Employer.Email ?? "", employerName, applicantName, job.Title);
        }

        return ApplicationResult.CreateSuccess(application);
    }

    public async Task<ApplicationResult> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus)
    {
        var application = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            return ApplicationResult.CreateError("Application not found");

        if (application.Status == newStatus)
            return ApplicationResult.CreateError($"Application is already {newStatus.ToString().ToLower()}");

        if (application.Status != ApplicationStatus.Pending)
            return ApplicationResult.CreateError($"Cannot change status from {application.Status} to {newStatus}");

        application.Status = newStatus;
        await _context.SaveChangesAsync();

        var applicantName = application.Applicant.UserName ?? application.Applicant.Email ?? "Applicant";
        await _notificationService.SendApplicationStatusChangedAsync(
            application.Applicant.Email ?? "",
            applicantName,
            application.Job.Title,
            newStatus);

        return ApplicationResult.CreateSuccess(application);
    }

    public async Task<bool> HasUserAppliedAsync(string userId, int jobId)
    {
        return await _applicationRepository.HasUserAppliedToJob(userId, jobId);
    }

    public async Task<IEnumerable<Application>> GetUserApplicationsAsync(string userId)
    {
        return await _applicationRepository.GetUserApplicationsAsync(userId);
    }

    public async Task<IEnumerable<Application>> GetJobApplicationsAsync(int jobId)
    {
        return await _applicationRepository.GetJobApplications(jobId);
    }
}
