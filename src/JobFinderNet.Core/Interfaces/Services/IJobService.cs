using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IJobService
{
    Task<ApplicationResult> ApplyForJob(int jobId, string userId);
    Task<bool> ApplyToJob(int jobId, string userId);
    Task<bool> CanUserApplyToJob(ApplicationUser user, int jobId);
    Task<ApplicationResult> CreateApplication(int jobId, string userId);
}
