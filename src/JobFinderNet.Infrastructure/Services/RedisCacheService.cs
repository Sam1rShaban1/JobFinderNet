using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes == null) return default;
        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        var options = new DistributedCacheEntryOptions();
        if (expiry.HasValue)
            options.SetAbsoluteExpiration(expiry.Value);
        await _cache.SetAsync(key, bytes, options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return _cache.RemoveAsync(key, ct);
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        // Full prefix-based flush would need direct StackExchange.Redis SCAN + DEL.
        // For now keys are invalidated individually in the calling code.
        return Task.CompletedTask;
    }
}
