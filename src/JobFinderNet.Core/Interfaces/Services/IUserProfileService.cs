using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfile> GetOrCreateProfileAsync(string userId);
    Task<UserProfile> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<List<object>> GetMatchedJobsAsync(string userId, int limit = 6);
    Task<List<MatchedJobDto>> GetMatchedJobsDetailedAsync(string userId, int limit = 12);
    Task<List<string>> GetAvailableSkillsAsync();
}
