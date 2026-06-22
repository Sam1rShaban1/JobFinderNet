using JobFinderNet.Core.DTOs;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IStatisticsService
{
    Task<JobStatisticsDto> GetDashboardStatisticsAsync();
    Task<EmployerDashboardDto> GetEmployerDashboardAsync(string employerId);
    Task<int> GetEmployerJobCountAsync(string employerId);
    Task<Dictionary<string, int>> GetApplicationsByJobAsync(string employerId);
    Task<PublicStatisticsDto> GetPublicStatisticsAsync();
}
