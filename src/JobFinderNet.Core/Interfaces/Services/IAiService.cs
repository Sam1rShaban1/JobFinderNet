using JobFinderNet.Core.DTOs;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IAiService
{
    Task<ParsedResume> ParseResumeAsync(ParseResumeRequest request);
    Task<List<MatchedJobDto>> GetRecommendationsAsync(string userId, ParsedResume resume, int limit = 10);
    Task<CoverLetterResponse> GenerateCoverLetterAsync(string userId, CoverLetterRequest request);
}
