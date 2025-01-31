using JobFinderNet.Models;

namespace JobFinderNet.Repositories;

public interface IApplicationRepository
{
    Task<bool> HasApplied(string userId, int jobId);
    Task AddAsync(JobApplication application);
    Task<List<JobApplication>> GetUserApplications(string userId);
    Task<List<JobApplication>> GetJobApplications(int jobId);
} 