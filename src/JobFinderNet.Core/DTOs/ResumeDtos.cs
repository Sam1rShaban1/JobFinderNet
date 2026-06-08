using System.ComponentModel.DataAnnotations;

namespace JobFinderNet.Core.DTOs;

public class ParseResumeRequest
{
    public string? ResumeText { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageMediaType { get; set; }
    public bool IsPdf { get; set; }
}

public class ParsedResume
{
    public List<string> Skills { get; set; } = [];
    public string? SeniorityLevel { get; set; }
    public int? ExperienceYears { get; set; }
    public List<ResumeEducation> Education { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
    public List<string> JobTitles { get; set; } = [];
}

public class ResumeEducation
{
    public string? Degree { get; set; }
    public string? Institution { get; set; }
    public string? Year { get; set; }
}

public class ResumeRecommendationDto
{
    public ParsedResume ParsedResume { get; set; } = new();
    public List<MatchedJobDto> Recommendations { get; set; } = [];
}

public class CoverLetterRequest
{
    [Required]
    public string JobTitle { get; set; } = string.Empty;
    [Required]
    public string CompanyName { get; set; } = string.Empty;
    public string? JobDescription { get; set; }
    public string? HiringManager { get; set; }
    public string? Tone { get; set; }
}

public class CoverLetterResponse
{
    public string CoverLetter { get; set; } = string.Empty;
    public string? Tips { get; set; }
}
