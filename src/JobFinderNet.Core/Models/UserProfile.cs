namespace JobFinderNet.Core.Models;

public class UserProfile
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public List<string> Skills { get; set; } = [];
    public string? SeniorityLevel { get; set; }
    public double? DesiredSalaryMin { get; set; }
    public double? DesiredSalaryMax { get; set; }
    public bool IsOpenToRemote { get; set; } = true;
    public string? PreferredLocation { get; set; }
    public string? PreferredJobType { get; set; }

    public bool EmailOnMatch { get; set; } = true;
    public int MinimumMatchScore { get; set; } = 20;
    public string EmailFrequency { get; set; } = "daily";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
