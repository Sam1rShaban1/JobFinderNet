using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Repositories;

public interface ISavedJobRepository
{
    Task<List<SavedJob>> GetUserSavedJobsAsync(string userId);
    Task<List<int>> GetUserSavedJobIdsAsync(string userId);
    Task<SavedJob?> GetAsync(string userId, int jobId);
    Task<bool> ExistsAsync(string userId, int jobId);
    Task AddAsync(SavedJob savedJob);
    void Remove(SavedJob savedJob);
    Task SaveChangesAsync();
}
