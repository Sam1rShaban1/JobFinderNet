using System.ComponentModel.DataAnnotations;

namespace JobFinderNet.Core.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }

    [Required]
    public string Role { get; set; } = "Applicant";
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}

public class CreateJobDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(10000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string? EmployerLogo { get; set; }
    public string? EmployerWebsite { get; set; }

    [Required]
    public string Location { get; set; } = string.Empty;

    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    [Required]
    public string JobType { get; set; } = "Full-time";

    [Required]
    public string Salary { get; set; } = string.Empty;

    public double? SalaryMin { get; set; }
    public double? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }
    public string? SalaryPeriod { get; set; }

    [Required]
    public string ExperienceRequired { get; set; } = "Entry Level";

    public int? RequiredExperienceYears { get; set; }
    public string? SeniorityLevel { get; set; }
    public string? Industry { get; set; }
    public string? JobFunction { get; set; }
    public string? WorkArrangement { get; set; }
    public string? ApplyLink { get; set; }
    public bool IsRemote { get; set; }
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
}

public class UpdateApplicationStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class MatchScoreBreakdown
{
    public int TechnologyScore { get; set; }
    public int SeniorityScore { get; set; }
    public int SalaryScore { get; set; }
    public int LocationScore { get; set; }
    public int JobTypeScore { get; set; }
    public int TotalScore { get; set; }
    public List<string> MatchedSkills { get; set; } = [];
    public List<string> MissingSkills { get; set; } = [];
    public string? SeniorityMatchReason { get; set; }
    public string? SalaryMatchReason { get; set; }
    public string? LocationMatchReason { get; set; }
    public string? JobTypeMatchReason { get; set; }
}

public class MatchedJobDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Salary { get; set; } = string.Empty;
    public string ExperienceRequired { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; }
    public bool IsRemote { get; set; }
    public int Score { get; set; }
    public MatchScoreBreakdown Breakdown { get; set; } = new();
}

public class SavedSearchDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Search { get; set; }
    public string? Location { get; set; }
    public string? JobType { get; set; }
    public double? SalaryMin { get; set; }
    public double? SalaryMax { get; set; }
    public bool? IsRemote { get; set; }
    public string? Seniority { get; set; }
    public List<string>? Tech { get; set; }
    public string EmailFrequency { get; set; } = "daily";
}

public class CreateCompanyProfileDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Size { get; set; }
    public string? Industry { get; set; }
}

public class AddNoteDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
