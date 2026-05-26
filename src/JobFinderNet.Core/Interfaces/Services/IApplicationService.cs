using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IApplicationService
{
    Task<ApplicationResult> SubmitApplicationAsync(int jobId, string userId, string? coverLetter = null);
    Task<ApplicationResult> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus);
    Task<bool> HasUserAppliedAsync(string userId, int jobId);
    Task<IEnumerable<Application>> GetUserApplicationsAsync(string userId);
    Task<IEnumerable<Application>> GetJobApplicationsAsync(int jobId);
}
