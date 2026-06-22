using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class SavedJobService : ISavedJobService
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IJobRepository _jobRepository;

    public SavedJobService(ISavedJobRepository savedJobRepository, IJobRepository jobRepository)
    {
        _savedJobRepository = savedJobRepository;
        _jobRepository = jobRepository;
    }

    public async Task<List<SavedJob>> GetUserSavedJobsAsync(string userId)
    {
        return await _savedJobRepository.GetUserSavedJobsAsync(userId);
    }

    public async Task<List<int>> GetUserSavedJobIdsAsync(string userId)
    {
        return await _savedJobRepository.GetUserSavedJobIdsAsync(userId);
    }

    public async Task SaveJobAsync(string userId, int jobId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
            throw new KeyNotFoundException("Job not found");

        var existing = await _savedJobRepository.GetAsync(userId, jobId);
        if (existing != null)
            return;

        var saved = new SavedJob
        {
            UserId = userId,
            JobId = jobId,
            SavedDate = DateTime.UtcNow
        };

        await _savedJobRepository.AddAsync(saved);
        await _savedJobRepository.SaveChangesAsync();
    }

    public async Task UnsaveJobAsync(string userId, int jobId)
    {
        var saved = await _savedJobRepository.GetAsync(userId, jobId);
        if (saved == null)
            throw new KeyNotFoundException("Saved job not found");

        _savedJobRepository.Remove(saved);
        await _savedJobRepository.SaveChangesAsync();
    }
}
