using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Repositories;

public interface ISavedSearchRepository
{
    Task<List<SavedSearch>> GetUserSavedSearchesAsync(string userId);
    Task<SavedSearch?> GetByIdAsync(int id);
    Task<SavedSearch?> GetByIdForUserAsync(int id, string userId);
    Task<UserProfile?> GetUserProfileAsync(string userId);
    Task AddAsync(SavedSearch savedSearch);
    void Update(SavedSearch savedSearch);
    void Remove(SavedSearch savedSearch);
    Task SaveChangesAsync();
}
