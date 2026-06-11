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

public class EmployerDashboardDto
{
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int TotalApplications { get; set; }
    public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
    public List<JobApplicationCount> TopJobs { get; set; } = new();
    public List<MonthlyJobPosting> MonthlyPostings { get; set; } = new();
}

public class JobApplicationCount
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ApplicationCount { get; set; }
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
    Task<EmployerDashboardDto> GetEmployerDashboardAsync(string employerId);
    Task<int> GetEmployerJobCountAsync(string employerId);
    Task<Dictionary<string, int>> GetApplicationsByJobAsync(string employerId);
}
