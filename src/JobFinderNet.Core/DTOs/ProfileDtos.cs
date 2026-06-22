using System.ComponentModel.DataAnnotations;

namespace JobFinderNet.Core.DTOs;

public class UpdateProfileDto
{
    public List<string>? Skills { get; set; }
    public string? SeniorityLevel { get; set; }
    public double? DesiredSalaryMin { get; set; }
    public double? DesiredSalaryMax { get; set; }
    public bool IsOpenToRemote { get; set; } = true;
    public string? PreferredLocation { get; set; }
    public string? PreferredJobType { get; set; }
    public bool EmailOnMatch { get; set; } = true;
    public int MinimumMatchScore { get; set; } = 70;
    public string? EmailFrequency { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string? SeniorityLevel { get; set; }
    public double? DesiredSalaryMin { get; set; }
    public double? DesiredSalaryMax { get; set; }
    public bool IsOpenToRemote { get; set; }
    public string? PreferredLocation { get; set; }
    public string? PreferredJobType { get; set; }
    public bool EmailOnMatch { get; set; }
    public int MinimumMatchScore { get; set; }
    public string? EmailFrequency { get; set; }
    public DateTime UpdatedAt { get; set; }
}
