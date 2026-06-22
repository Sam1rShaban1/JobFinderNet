using System.Text.Json;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class SavedSearchService : ISavedSearchService
{
    private readonly ISavedSearchRepository _savedSearchRepository;
    private readonly IMatchingService _matchingService;

    public SavedSearchService(ISavedSearchRepository savedSearchRepository, IMatchingService matchingService)
    {
        _savedSearchRepository = savedSearchRepository;
        _matchingService = matchingService;
    }

    public async Task<List<SavedSearch>> GetUserSavedSearchesAsync(string userId)
    {
        return await _savedSearchRepository.GetUserSavedSearchesAsync(userId);
    }

    public async Task<SavedSearch> CreateSavedSearchAsync(string userId, SavedSearchDto dto)
    {
        var profile = await _savedSearchRepository.GetUserProfileAsync(userId);
        if (profile == null)
            throw new InvalidOperationException("Create a profile first");

        var filtersJson = JsonSerializer.Serialize(new
        {
            search = dto.Search,
            location = dto.Location,
            jobType = dto.JobType,
            salaryMin = dto.SalaryMin,
            salaryMax = dto.SalaryMax,
            isRemote = dto.IsRemote,
            seniority = dto.Seniority,
            tech = dto.Tech
        });

        var savedSearch = new SavedSearch
        {
            UserId = userId,
            Name = dto.Name,
            FiltersJson = filtersJson,
            EmailFrequency = dto.EmailFrequency,
            CreatedAt = DateTime.UtcNow
        };

        await _savedSearchRepository.AddAsync(savedSearch);
        await _savedSearchRepository.SaveChangesAsync();

        return savedSearch;
    }

    public async Task<SavedSearch?> UpdateSavedSearchAsync(int id, string userId, SavedSearchDto dto)
    {
        var savedSearch = await _savedSearchRepository.GetByIdForUserAsync(id, userId);
        if (savedSearch == null)
            return null;

        var filtersJson = JsonSerializer.Serialize(new
        {
            search = dto.Search,
            location = dto.Location,
            jobType = dto.JobType,
            salaryMin = dto.SalaryMin,
            salaryMax = dto.SalaryMax,
            isRemote = dto.IsRemote,
            seniority = dto.Seniority,
            tech = dto.Tech
        });

        savedSearch.Name = dto.Name;
        savedSearch.FiltersJson = filtersJson;
        savedSearch.EmailFrequency = dto.EmailFrequency;

        await _savedSearchRepository.SaveChangesAsync();
        return savedSearch;
    }

    public async Task DeleteSavedSearchAsync(int id, string userId)
    {
        var savedSearch = await _savedSearchRepository.GetByIdForUserAsync(id, userId);
        if (savedSearch == null)
            throw new KeyNotFoundException("Saved search not found");

        _savedSearchRepository.Remove(savedSearch);
        await _savedSearchRepository.SaveChangesAsync();
    }

    public async Task<object?> RunSavedSearchAsync(int id, string userId)
    {
        var savedSearch = await _savedSearchRepository.GetByIdForUserAsync(id, userId);
        if (savedSearch == null)
            return null;

        var profile = await _savedSearchRepository.GetUserProfileAsync(userId);
        if (profile == null)
            throw new InvalidOperationException("Profile not found");

        var matches = await _matchingService.GetTopMatches(profile, 10);

        savedSearch.LastRunAt = DateTime.UtcNow;
        await _savedSearchRepository.SaveChangesAsync();

        return new
        {
            searchName = savedSearch.Name,
            matchCount = matches.Count,
            matches = matches.Select(m => new
            {
                m.Job.Id,
                m.Job.Title,
                m.Job.CompanyName,
                m.Job.Location,
                Score = m.Score
            })
        };
    }
}
