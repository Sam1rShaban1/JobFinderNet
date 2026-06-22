using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IJobService
{
    Task<Job?> GetByIdAsync(int id);
    Task<PaginatedList<Job>> GetPaginatedJobsAsync(int page, int pageSize);
    Task<List<Job>> SearchJobsAsync(string query);
    Task<List<Job>> GetEmployerJobsAsync(string employerId);
    Task<Job> CreateJobAsync(CreateJobDto dto, string employerId);
    Task<Job?> UpdateJobAsync(int id, CreateJobDto dto, string employerId);
    Task DeleteJobAsync(int id, string employerId);
    Task ToggleJobStatusAsync(int id, string employerId);
    Task<List<Job>> GetSimilarJobsAsync(int id);
    Task PopulateTechnologiesAsync();
    Task<List<Application>> GetJobApplicationsAsync(int jobId, string employerId);
}
