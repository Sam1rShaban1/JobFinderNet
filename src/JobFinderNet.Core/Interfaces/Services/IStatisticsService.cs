namespace JobFinderNet.Core.Interfaces.Services;

public class JobStatisticsDto
{
    public int TotalActiveJobs { get; set; }
    public int TotalApplications { get; set; }
    public int TotalEmployers { get; set; }
    public int TotalApplicants { get; set; }
    public Dictionary<string, int> JobsByType { get; set; } = new();
    public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
    public List<MonthlyJobPosting> MonthlyJobPostings { get; set; } = new();
}

public class MonthlyJobPosting
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public interface IStatisticsService
{
    Task<JobStatisticsDto> GetDashboardStatisticsAsync();
    Task<int> GetEmployerJobCountAsync(string employerId);
    Task<Dictionary<string, int>> GetApplicationsByJobAsync(string employerId);
}
