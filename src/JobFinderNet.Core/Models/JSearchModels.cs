using System.Text.Json.Serialization;

namespace JobFinderNet.Core.Models;

public class JSearchResponse
{
    [JsonPropertyName("data")]
    public List<JSearchJob> Data { get; init; } = [];
}

public class JSearchJob
{
    [JsonPropertyName("job_title")]
    public string JobTitle { get; init; } = "";

    [JsonPropertyName("employer_name")]
    public string EmployerName { get; init; } = "";

    [JsonPropertyName("employer_logo")]
    public string? EmployerLogo { get; init; }

    [JsonPropertyName("job_description")]
    public string JobDescription { get; init; } = "";

    [JsonPropertyName("job_city")]
    public string? JobCity { get; init; }

    [JsonPropertyName("job_state")]
    public string? JobState { get; init; }

    [JsonPropertyName("job_country")]
    public string? JobCountry { get; init; }

    [JsonPropertyName("job_employment_type")]
    public string? JobEmploymentType { get; init; }

    [JsonPropertyName("job_min_salary")]
    public double? JobMinSalary { get; init; }

    [JsonPropertyName("job_max_salary")]
    public double? JobMaxSalary { get; init; }

    [JsonPropertyName("job_posted_at_datetime_utc")]
    public DateTime? JobPostedAt { get; init; }

    [JsonPropertyName("job_required_experience")]
    public JSearchExperience? RequiredExperience { get; init; }

    [JsonPropertyName("job_apply_link")]
    public string? JobApplyLink { get; init; }
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
