using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Repositories;

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
    Task<PaginatedList<Job>> GetPaginatedJobsAsync(int pageIndex, int pageSize);
    Task<List<Job>> SearchJobsAsync(string query);
    Task<List<Job>> GetRecentJobsAsync(int count);
    Task DeleteJobAsync(int id);
}
