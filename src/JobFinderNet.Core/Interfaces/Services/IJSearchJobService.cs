namespace JobFinderNet.Core.Interfaces.Services;

public interface IJSearchJobService
{
    Task<int> SyncJobsAsync(CancellationToken ct = default);
}
