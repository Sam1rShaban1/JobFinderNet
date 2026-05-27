using System.Text.Json;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Repositories;

public class CachedJobRepository : IJobRepository
{
    private readonly IJobRepository _inner;
    private readonly ICacheService _cache;
    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DetailCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(1);
    private const string JobsPrefix = "jobs:";

    public CachedJobRepository(IJobRepository inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<PaginatedList<Job>> GetPaginatedJobsAsync(int pageIndex, int pageSize)
    {
        var key = $"{JobsPrefix}page:{pageIndex}:size:{pageSize}";
        var cached = await _cache.GetAsync<PaginatedList<Job>>(key);
        if (cached != null) return cached;

        var result = await _inner.GetPaginatedJobsAsync(pageIndex, pageSize);
        await _cache.SetAsync(key, result, ListCacheDuration);
        return result;
    }

    public async Task<List<Job>> SearchJobsAsync(string query)
    {
        var normalized = query.Trim().ToLowerInvariant();
        var key = $"{JobsPrefix}search:{normalized}";
        var cached = await _cache.GetAsync<List<Job>>(key);
        if (cached != null) return cached;

        var result = await _inner.SearchJobsAsync(query);
        await _cache.SetAsync(key, result, SearchCacheDuration);
        return result;
    }

    public async Task<List<Job>> GetRecentJobsAsync(int count)
    {
        var key = $"{JobsPrefix}recent:{count}";
        var cached = await _cache.GetAsync<List<Job>>(key);
        if (cached != null) return cached;

        var result = await _inner.GetRecentJobsAsync(count);
        await _cache.SetAsync(key, result, ListCacheDuration);
        return result;
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        var key = $"{JobsPrefix}detail:{id}";
        var cached = await _cache.GetAsync<Job>(key);
        if (cached != null) return cached;

        var result = await _inner.GetByIdAsync(id);
        if (result != null)
            await _cache.SetAsync(key, result, DetailCacheDuration);
        return result;
    }

    public async Task<IEnumerable<Job>> GetActiveJobsAsync(int page, int pageSize)
    {
        var key = $"{JobsPrefix}active:{page}:{pageSize}";
        var cached = await _cache.GetAsync<List<Job>>(key);
        if (cached != null) return cached;

        var result = await _inner.GetActiveJobsAsync(page, pageSize);
        await _cache.SetAsync(key, result.ToList(), ListCacheDuration);
        return result;
    }

    public async Task<int> GetTotalActiveJobsCount()
    {
        var key = $"{JobsPrefix}active:count";
        var cached = await _cache.GetAsync<int?>(key);
        if (cached is not null) return cached.Value;

        var result = await _inner.GetTotalActiveJobsCount();
        await _cache.SetAsync(key, result, ListCacheDuration);
        return result;
    }

    // Write-through: invalidate caches on mutations
    public async Task CreateJobAsync(Job job)
    {
        await _inner.CreateJobAsync(job);
        await InvalidateJobListsAsync();
    }

    public async Task UpdateJobAsync(Job job)
    {
        await _inner.UpdateJobAsync(job);
        await InvalidateJobListsAsync();
        await _cache.RemoveAsync($"{JobsPrefix}detail:{job.Id}");
    }

    public async Task ToggleJobStatusAsync(int id)
    {
        await _inner.ToggleJobStatusAsync(id);
        await InvalidateJobListsAsync();
        await _cache.RemoveAsync($"{JobsPrefix}detail:{id}");
    }

    public async Task DeleteJobAsync(int id)
    {
        await _inner.DeleteJobAsync(id);
        await InvalidateJobListsAsync();
        await _cache.RemoveAsync($"{JobsPrefix}detail:{id}");
    }

    public async Task<List<Job>> GetEmployerJobsAsync(string employerId)
    {
        var key = $"{JobsPrefix}employer:{employerId}";
        var cached = await _cache.GetAsync<List<Job>>(key);
        if (cached != null) return cached;

        var result = await _inner.GetEmployerJobsAsync(employerId);
        await _cache.SetAsync(key, result, ListCacheDuration);
        return result;
    }

    public async Task<bool> JobExists(int id)
    {
        // No caching needed for simple existence check
        return await _inner.JobExists(id);
    }

    private async Task InvalidateJobListsAsync()
    {
        // Remove known list cache keys by their full keys.
        // A more robust approach would use Redis SCAN + DEL via StackExchange.Raw,
        // but this covers the common paths without extra dependencies.
        var keysToRemove = new[]
        {
            $"{JobsPrefix}active:count",
        };
        foreach (var key in keysToRemove)
            await _cache.RemoveAsync(key);
    }
}
