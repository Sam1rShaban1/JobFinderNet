using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class JSearchOptions
{
    public const string SectionName = "JSearch";
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openwebninja.com/jsearch";
    public List<string> Queries { get; set; } =
    [
        "software engineer",
        "data analyst",
        "product manager",
        "full stack developer",
        "devops engineer",
        "data scientist",
        "ux designer",
        "project manager",
        "sales engineer",
        "backend engineer",
        "frontend developer",
        "machine learning engineer",
        "cloud architect",
        "security engineer",
        "mobile developer",
    ];
    public int MaxPages { get; set; } = 5;
}

public class JSearchJobService : IJSearchJobService
{
    private readonly JSearchOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JSearchJobService> _logger;

    public JSearchJobService(
        IOptions<JSearchOptions> options,
        HttpClient httpClient,
        IServiceScopeFactory scopeFactory,
        ILogger<JSearchJobService> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<int> SyncJobsAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("JSearch API key not configured, skipping sync");
            return 0;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var systemEmployerId = await DataSeeder.GetOrCreateSystemEmployer(userManager);
        var totalAdded = 0;

        foreach (var query in _options.Queries)
        {
            if (ct.IsCancellationRequested) break;

            var cursor = (string?)null;
            for (int page = 0; page < _options.MaxPages; page++)
            {
                var (jobs, nextCursor) = await FetchPageAsync(query, cursor, ct);
                if (jobs.Count == 0) break;

                foreach (var jSearchJob in jobs)
                {
                    if (string.IsNullOrEmpty(jSearchJob.JobId)) continue;

                    var exists = await context.Jobs.AnyAsync(j => j.ExternalJobId == jSearchJob.JobId, ct);
                    if (exists) continue;

                    context.Jobs.Add(MapToJob(jSearchJob, systemEmployerId));
                    totalAdded++;
                }

                await context.SaveChangesAsync(ct);

                if (string.IsNullOrEmpty(nextCursor)) break;
                cursor = nextCursor;
            }
        }

        _logger.LogInformation("JSearch sync completed: {Count} new jobs added", totalAdded);
        return totalAdded;
    }

    private async Task<(List<JSearchJob> Jobs, string? NextCursor)> FetchPageAsync(
        string query, string? cursor, CancellationToken ct)
    {
        try
        {
            var url = $"{_options.BaseUrl}/search-v2?query={Uri.EscapeDataString(query)}&num_pages=5";
            if (!string.IsNullOrEmpty(cursor))
                url += $"&cursor={Uri.EscapeDataString(cursor)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-Key", _options.ApiKey);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            List<JSearchJob>? jobs = null;
            string? nextCursor = null;

            // Try v2 format: data.jobs + data.cursor
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            {
                if (data.TryGetProperty("jobs", out var jobsElement) && jobsElement.ValueKind == JsonValueKind.Array)
                {
                    jobs = JsonSerializer.Deserialize<List<JSearchJob>>(jobsElement.GetRawText());
                }
                if (data.TryGetProperty("cursor", out var cursorElement))
                {
                    nextCursor = cursorElement.GetString();
                }
            }
            // Fallback to v1 format: data[] (direct array)
            else if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
            {
                jobs = JsonSerializer.Deserialize<List<JSearchJob>>(dataArray.GetRawText());
            }

            return (jobs ?? [], nextCursor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JSearch API request failed for query '{Query}' cursor '{Cursor}'",
                query, cursor);
            return ([], null);
        }
    }

    private static Job MapToJob(JSearchJob source, string employerId)
    {
        return new Job
        {
            Title = Truncate(source.JobTitle, 200),
            Description = source.JobDescription,
            CompanyName = Truncate(source.EmployerName, 200),
            EmployerLogo = source.EmployerLogo,
            EmployerWebsite = source.EmployerWebsite,
            JobPublisher = source.JobPublisher,
            Location = FormatLocation(source),
            City = source.JobCity,
            State = source.JobState,
            Country = source.JobCountry,
            Latitude = source.JobLatitude,
            Longitude = source.JobLongitude,
            JobType = MapJobType(source.JobEmploymentType),
            Salary = FormatSalary(source.JobMinSalary, source.JobMaxSalary),
            SalaryMin = source.JobMinSalary,
            SalaryMax = source.JobMaxSalary,
            SalaryCurrency = "USD",
            SalaryPeriod = source.JobSalaryPeriod,
            ExperienceRequired = MapExperience(source.RequiredExperienceYears ?? 0),
            RequiredExperienceYears = source.RequiredExperienceYears,
            SeniorityLevel = source.SeniorityLevel,
            Industry = source.Industry,
            JobFunction = source.JobFunction,
            WorkArrangement = source.WorkArrangement,
            ExternalJobId = source.JobId,
            ApplyLink = source.JobApplyLink,
            IsRemote = source.JobIsRemote ?? false,
            IsActive = true,
            PostedAtTimestamp = source.JobPostedAtTimestamp,
            PostedDate = source.JobPostedAtDatetimeUtc?.ToUniversalTime() ?? DateTime.UtcNow,
            HasManagementResponsibilities = source.HasManagementResponsibilities,
            IsAiMlInvolved = source.AiMlInvolved,
            EducationRequired = source.EducationRequired,
            ContractDuration = source.ContractDuration,
            RequiredTechnologies = source.RequiredTechnologies ?? [],
            PreferredTechnologies = source.PreferredTechnologies ?? [],
            SoftSkills = source.SoftSkills ?? [],
            Benefits = source.JobBenefits ?? [],
            Methodologies = source.Methodologies ?? [],
            HighlightsQualifications = FormatHighlights(source.JobHighlights?.Qualifications),
            HighlightsResponsibilities = FormatHighlights(source.JobHighlights?.Responsibilities),
            HighlightsBenefits = FormatHighlights(source.JobHighlights?.Benefits),
            Source = "JSearch",
            SourceUrl = source.JobApplyLink,
            EmployerId = employerId,
            Employer = null!,
        };
    }

    private static string FormatLocation(JSearchJob job)
    {
        if (!string.IsNullOrEmpty(job.JobLocation))
            return job.JobLocation;

        var parts = new[] { job.JobCity, job.JobState, job.JobCountry };
        return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    private static string MapJobType(string? type)
    {
        return type?.ToUpperInvariant() switch
        {
            "FULL_TIME" => "Full-time",
            "PART_TIME" => "Part-time",
            "CONTRACT" => "Contract",
            "INTERN" or "INTERNSHIP" => "Internship",
            "TEMPORARY" => "Temporary",
            _ => "Full-time",
        };
    }

    private static string FormatSalary(double? min, double? max)
    {
        if (min == null && max == null) return "Competitive";
        if (min == null) return $"Up to ${max:F0}/year";
        if (max == null) return $"From ${min:F0}/year";
        return $"${min:F0} - ${max:F0}/year";
    }

    private static string MapExperience(int? years)
    {
        if (years == null || years <= 0) return "Not specified";
        return years switch
        {
            <= 1 => "Entry Level",
            <= 2 => "1-2 years",
            <= 3 => "2-3 years",
            <= 5 => "3-5 years",
            _ => "5+ years",
        };
    }

    private static string FormatHighlights(List<string>? items)
    {
        if (items == null || items.Count == 0) return "";
        return string.Join("\n", items);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
