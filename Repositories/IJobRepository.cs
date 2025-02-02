using JobFinderNet.Models;

namespace JobFinderNet.Repositories;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(int id);
    Task<IEnumerable<Job>> GetActiveJobsAsync(int page, int pageSize);
    Task<int> GetTotalActiveJobsCount();
    Task<List<Job>> GetEmployerJobsAsync(string employerId);
    Task CreateJobAsync(Job job);
    Task UpdateJobAsync(Job job);
    Task ToggleJobStatusAsync(int id);
    Task<bool> JobExists(int id);
    Task<bool> ApplyForJobAsync(int jobId, string userId);
    Task<PaginatedList<Job>> GetPaginatedJobsAsync(int pageIndex, int pageSize);
    Task<List<Job>> SearchJobsAsync(string query);
    Task<List<Job>> GetRecentJobsAsync(int count);
    Task DeleteJobAsync(int id);
} 