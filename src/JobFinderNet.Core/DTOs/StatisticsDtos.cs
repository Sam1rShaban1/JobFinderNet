namespace JobFinderNet.Core.DTOs;

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

public class PublicStatisticsDto
{
    public int TotalJobs { get; set; }
    public int TotalUsers { get; set; }
    public int TotalApplications { get; set; }
    public int JobsWithTech { get; set; }
    public int TotalTechnologies { get; set; }
    public Dictionary<string, int> JobsByType { get; set; } = new();
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Link { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CompanyProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Size { get; set; }
    public string? Industry { get; set; }
    public bool IsVerified { get; set; }
    public int OpenRoles { get; set; }
}

public class CompanySearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public int OpenRoles { get; set; }
}
