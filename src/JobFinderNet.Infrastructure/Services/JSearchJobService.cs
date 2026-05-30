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
    public string ApiHost { get; set; } = "jsearch.p.rapidapi.com";
    public string BaseUrl { get; set; } = "https://jsearch.p.rapidapi.com";
    public List<string> Queries { get; set; } = ["software engineer", "data analyst", "product manager"];
    public int MaxPages { get; set; } = 2;
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

            for (int page = 1; page <= _options.MaxPages; page++)
            {
                var jobs = await FetchPageAsync(query, page, ct);
                if (jobs.Count == 0) break;

                foreach (var jSearchJob in jobs)
                {
                    if (await context.Jobs.AnyAsync(j => j.Title == jSearchJob.JobTitle
                        && j.CompanyName == jSearchJob.EmployerName
                        && j.Location == FormatLocation(jSearchJob), ct)) continue;

                    context.Jobs.Add(MapToJob(jSearchJob, systemEmployerId));
                    totalAdded++;
                }

                await context.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("JSearch sync completed: {Count} new jobs added", totalAdded);
        return totalAdded;
    }

    private async Task<List<JSearchJob>> FetchPageAsync(string query, int page, CancellationToken ct)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_options.BaseUrl}/search?query={Uri.EscapeDataString(query)}&page={page}&num_pages=1");

            request.Headers.Add("x-rapidapi-key", _options.ApiKey);
            request.Headers.Add("x-rapidapi-host", _options.ApiHost);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JSearchResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JSearch API request failed for query '{Query}' page {Page}", query, page);
            return [];
        }
    }

    private static Job MapToJob(JSearchJob source, string employerId)
    {
        return new Job
        {
            Title = Truncate(source.JobTitle, 100),
            Description = Truncate(source.JobDescription, 1000),
            CompanyName = Truncate(source.EmployerName, 100),
            Location = FormatLocation(source),
            JobType = MapJobType(source.JobEmploymentType),
            Salary = FormatSalary(source.JobMinSalary, source.JobMaxSalary),
            ExperienceRequired = MapExperience(source.RequiredExperience),
            IsActive = true,
            PostedDate = source.JobPostedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            EmployerId = employerId,
            Employer = null!,
        };
    }

    private static string FormatLocation(JSearchJob job)
    {
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

    private static string MapExperience(JSearchExperience? exp)
    {
        if (exp?.RequiredExperienceInMonths == null) return "Not specified";
        var months = exp.RequiredExperienceInMonths.Value;
        return months switch
        {
            <= 0 => "Entry Level",
            <= 12 => "1 year",
            <= 24 => "1-2 years",
            <= 36 => "2-3 years",
            <= 60 => "3-5 years",
            _ => "5+ years",
        };
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
