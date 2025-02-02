using JobFinderNet.Models;

namespace JobFinderNet.Repositories;

public interface IApplicationRepository
{
    Task<bool> HasUserAppliedToJob(string userId, int jobId);
    Task<bool> AddAsync(Application application);
    Task<IEnumerable<Application>> GetUserApplicationsAsync(string userId);
    Task<List<Application>> GetJobApplications(int jobId);
} 