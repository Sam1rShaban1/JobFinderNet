using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface ISavedJobService
{
    Task<List<SavedJob>> GetUserSavedJobsAsync(string userId);
    Task<List<int>> GetUserSavedJobIdsAsync(string userId);
    Task SaveJobAsync(string userId, int jobId);
    Task UnsaveJobAsync(string userId, int jobId);
}
