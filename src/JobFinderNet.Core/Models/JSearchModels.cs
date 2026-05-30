using System.Text.Json.Serialization;

namespace JobFinderNet.Core.Models;

public class JSearchResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; init; }

    [JsonPropertyName("data")]
    public JSearchResponseData? Data { get; init; }
}

public class JSearchResponseData
{
    [JsonPropertyName("jobs")]
    public List<JSearchJob>? Jobs { get; init; }

    [JsonPropertyName("cursor")]
    public string? Cursor { get; init; }
}

public class JSearchJob
{
    [JsonPropertyName("job_id")]
    public string? JobId { get; init; }

    [JsonPropertyName("job_title")]
    public string JobTitle { get; init; } = "";

    [JsonPropertyName("employer_name")]
    public string EmployerName { get; init; } = "";

    [JsonPropertyName("employer_logo")]
    public string? EmployerLogo { get; init; }

    [JsonPropertyName("employer_website")]
    public string? EmployerWebsite { get; init; }

    [JsonPropertyName("job_publisher")]
    public string? JobPublisher { get; init; }

    [JsonPropertyName("job_employment_type")]
    public string? JobEmploymentType { get; init; }

    [JsonPropertyName("job_employment_types")]
    public List<string>? JobEmploymentTypes { get; init; }

    [JsonPropertyName("job_apply_link")]
    public string? JobApplyLink { get; init; }

    [JsonPropertyName("job_apply_is_direct")]
    public bool? JobApplyIsDirect { get; init; }

    [JsonPropertyName("job_description")]
    public string JobDescription { get; init; } = "";

    [JsonPropertyName("job_is_remote")]
    public bool? JobIsRemote { get; init; }

    [JsonPropertyName("job_posted_at")]
    public string? JobPostedAt { get; init; }

    [JsonPropertyName("job_posted_at_timestamp")]
    public long? JobPostedAtTimestamp { get; init; }

    [JsonPropertyName("job_posted_at_datetime_utc")]
    public DateTime? JobPostedAtDatetimeUtc { get; init; }

    [JsonPropertyName("job_location")]
    public string? JobLocation { get; init; }

    [JsonPropertyName("job_city")]
    public string? JobCity { get; init; }

    [JsonPropertyName("job_state")]
    public string? JobState { get; init; }

    [JsonPropertyName("job_country")]
    public string? JobCountry { get; init; }

    [JsonPropertyName("job_latitude")]
    public double? JobLatitude { get; init; }

    [JsonPropertyName("job_longitude")]
    public double? JobLongitude { get; init; }

    [JsonPropertyName("job_benefits")]
    public List<string>? JobBenefits { get; init; }

    [JsonPropertyName("job_min_salary")]
    public double? JobMinSalary { get; init; }

    [JsonPropertyName("job_max_salary")]
    public double? JobMaxSalary { get; init; }

    [JsonPropertyName("job_salary_period")]
    public string? JobSalaryPeriod { get; init; }

    [JsonPropertyName("job_highlights")]
    public JSearchHighlights? JobHighlights { get; init; }

    [JsonPropertyName("work_arrangement")]
    public string? WorkArrangement { get; init; }

    [JsonPropertyName("seniority_level")]
    public string? SeniorityLevel { get; init; }

    [JsonPropertyName("required_experience_years")]
    public int? RequiredExperienceYears { get; init; }

    [JsonPropertyName("education_required")]
    public string? EducationRequired { get; init; }

    [JsonPropertyName("visa_sponsorship")]
    public bool? VisaSponsorship { get; init; }

    [JsonPropertyName("relocation_required")]
    public bool? RelocationRequired { get; init; }

    [JsonPropertyName("contract_duration")]
    public string? ContractDuration { get; init; }

    [JsonPropertyName("required_technologies")]
    public List<string>? RequiredTechnologies { get; init; }

    [JsonPropertyName("preferred_technologies")]
    public List<string>? PreferredTechnologies { get; init; }

    [JsonPropertyName("methodologies")]
    public List<string>? Methodologies { get; init; }

    [JsonPropertyName("industry")]
    public string? Industry { get; init; }

    [JsonPropertyName("job_function")]
    public string? JobFunction { get; init; }

    [JsonPropertyName("has_management_responsibilities")]
    public bool? HasManagementResponsibilities { get; init; }

    [JsonPropertyName("ai_ml_involved")]
    public bool? AiMlInvolved { get; init; }

    [JsonPropertyName("benefits_extended")]
    public List<string>? BenefitsExtended { get; init; }

    [JsonPropertyName("soft_skills")]
    public List<string>? SoftSkills { get; init; }

    [JsonPropertyName("job_required_experience")]
    public JSearchExperience? RequiredExperience { get; init; }
}

public class JSearchHighlights
{
    [JsonPropertyName("Qualifications")]
    public List<string>? Qualifications { get; init; }

    [JsonPropertyName("Responsibilities")]
    public List<string>? Responsibilities { get; init; }

    [JsonPropertyName("Benefits")]
    public List<string>? Benefits { get; init; }
}

public class JSearchExperience
{
    [JsonPropertyName("required_experience_in_months")]
    public int? RequiredExperienceInMonths { get; init; }

    [JsonPropertyName("experience_mentioned")]
    public bool ExperienceMentioned { get; init; }

    [JsonPropertyName("experience_preferred")]
    public bool ExperiencePreferred { get; init; }
}
