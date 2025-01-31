namespace JobFinderNet.Repositories;

public interface IJobRepository
{
    Task<Job> GetByIdAsync(int id);
    Task<List<Job>> GetActiveJobsAsync();
    Task CreateJobAsync(Job job);
    Task UpdateJobAsync(Job job);
    Task ToggleJobStatusAsync(int id);
    Task<bool> JobExists(int id);
    Task<bool> ApplyForJobAsync(int jobId, string userId);
} 