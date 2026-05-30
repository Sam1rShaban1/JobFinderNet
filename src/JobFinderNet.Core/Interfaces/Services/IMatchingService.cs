using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IMatchingService
{
    Task<int> CalculateMatchScore(Job job, UserProfile profile);
    Task<List<(Job Job, int Score)>> GetTopMatches(UserProfile profile, int limit = 10);
    Task<List<(Job Job, int Score)>> GetTopMatchesAboveThreshold(UserProfile profile, int limit = 10);
}
