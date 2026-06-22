using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface ISavedSearchService
{
    Task<List<SavedSearch>> GetUserSavedSearchesAsync(string userId);
    Task<SavedSearch> CreateSavedSearchAsync(string userId, SavedSearchDto dto);
    Task<SavedSearch?> UpdateSavedSearchAsync(int id, string userId, SavedSearchDto dto);
    Task DeleteSavedSearchAsync(int id, string userId);
    Task<object?> RunSavedSearchAsync(int id, string userId);
}
