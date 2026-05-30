namespace JobFinderNet.Core.Models;

public class Job
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string CompanyName { get; set; }
    public string? EmployerLogo { get; set; }
    public string? EmployerWebsite { get; set; }
    public string? JobPublisher { get; set; }
    public required string Location { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public required string JobType { get; set; }
    public required string Salary { get; set; }
    public double? SalaryMin { get; set; }
    public double? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public string? SalaryPeriod { get; set; }
    public required string ExperienceRequired { get; set; }
    public int? RequiredExperienceYears { get; set; }
    public string? SeniorityLevel { get; set; }
    public string? Industry { get; set; }
    public string? JobFunction { get; set; }
    public string? WorkArrangement { get; set; }
    public string? ExternalJobId { get; set; }
    public string? ApplyLink { get; set; }
    public bool IsRemote { get; set; }
    public bool IsActive { get; set; } = true;
    public long? PostedAtTimestamp { get; set; }
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    public bool? HasManagementResponsibilities { get; set; }
    public bool? IsAiMlInvolved { get; set; }
    public string? EducationRequired { get; set; }
    public string? ContractDuration { get; set; }
    public List<string> RequiredTechnologies { get; set; } = [];
    public List<string> PreferredTechnologies { get; set; } = [];
    public List<string> SoftSkills { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public List<string> Methodologies { get; set; } = [];
    public string? HighlightsQualifications { get; set; }
    public string? HighlightsResponsibilities { get; set; }
    public string? HighlightsBenefits { get; set; }
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }

    public required string EmployerId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public ApplicationUser Employer { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
