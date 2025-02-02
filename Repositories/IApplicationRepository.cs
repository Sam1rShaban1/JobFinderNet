using JobFinderNet.Models;

namespace JobFinderNet.Repositories;

public interface IApplicationRepository
{
    Task<bool> HasApplied(string userId, int jobId);
    Task<bool> AddAsync(Application application);
    Task<List<Application>> GetUserApplications(string userId);
    Task<List<Application>> GetJobApplications(int jobId);
} 