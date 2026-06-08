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

    public ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
}

public class SavedSearch
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public string FiltersJson { get; set; } = "{}";
    public string EmailFrequency { get; set; } = "daily";
    public DateTime? LastRunAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonIgnore]
    public UserProfile UserProfile { get; set; } = null!;
}
