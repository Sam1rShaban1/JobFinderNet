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
    private readonly IApplicationNoteRepository _applicationNoteRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationService(
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        IApplicationNoteRepository applicationNoteRepository,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _applicationNoteRepository = applicationNoteRepository;
        _notificationRepository = notificationRepository;
        _userManager = userManager;
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
        await _notificationRepository.AddAsync(new AppNotification
        {
            UserId = job.EmployerId,
            Title = "New Application",
            Message = $"{applicantName} applied to {job.Title}",
            Link = $"/jobs/{job.Id}",
            CreatedAt = DateTime.UtcNow
        });
        await _notificationRepository.SaveChangesAsync();

        return ApplicationResult.CreateSuccess(application);
    }

    public async Task<ApplicationResult> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
            return ApplicationResult.CreateError("Application not found");

        application.Status = newStatus;
        await _applicationRepository.AddAsync(application);

        var statusLabel = newStatus switch
        {
            ApplicationStatus.Accepted => "accepted",
            ApplicationStatus.Rejected => "not selected",
            ApplicationStatus.Screening => "moved to screening",
            ApplicationStatus.Interview => "moved to interview",
            _ => newStatus.ToString().ToLower()
        };

        await _notificationRepository.AddAsync(new AppNotification
        {
            UserId = application.ApplicantId,
            Title = "Application Update",
            Message = $"Your application has been {statusLabel}",
            Link = "/my-applications",
            CreatedAt = DateTime.UtcNow
        });
        await _notificationRepository.SaveChangesAsync();

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

    public async Task<List<ApplicationNote>> GetNotesAsync(int applicationId)
    {
        return await _applicationNoteRepository.GetByApplicationIdAsync(applicationId);
    }

    public async Task<ApplicationNote?> AddNoteAsync(int applicationId, string userId, string content)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null) return null;

        var note = new ApplicationNote
        {
            ApplicationId = applicationId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _applicationNoteRepository.AddAsync(note);
        return note;
    }
}
