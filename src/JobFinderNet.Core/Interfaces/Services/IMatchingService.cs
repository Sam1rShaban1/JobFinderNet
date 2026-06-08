using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IMatchingService
{
    Task<int> CalculateMatchScore(Job job, UserProfile profile);
    Task<MatchScoreBreakdown> CalculateMatchScoreDetailed(Job job, UserProfile profile);
    Task<List<(Job Job, int Score)>> GetTopMatches(UserProfile profile, int limit = 10);
    Task<List<(Job Job, int Score)>> GetTopMatchesAboveThreshold(UserProfile profile, int limit = 10);
    Task<List<MatchedJobDto>> GetTopMatchesDetailed(UserProfile profile, int limit = 10);
}
