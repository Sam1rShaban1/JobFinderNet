using System.Globalization;
using System.Text.RegularExpressions;
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

                    var existing = await context.Jobs.FirstOrDefaultAsync(j => j.ExternalJobId == jSearchJob.JobId, ct);
                    if (existing == null)
                    {
                        context.Jobs.Add(MapToJob(jSearchJob, systemEmployerId));
                        totalAdded++;
                    }
                    else if (existing.RequiredTechnologies.Count == 0 && existing.PreferredTechnologies.Count == 0)
                    {
                        var (required, preferred) = ExtractTechnologies($"{jSearchJob.JobTitle} {jSearchJob.JobDescription}");
                        existing.RequiredTechnologies = required;
                        existing.PreferredTechnologies = preferred;
                    }
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
        var requiredTechs = source.RequiredTechnologies ?? [];
        var preferredTechs = source.PreferredTechnologies ?? [];

        if (requiredTechs.Count == 0 && preferredTechs.Count == 0)
        {
            var text = $"{source.JobTitle} {source.JobDescription}";
            var (extracted, _) = ExtractTechnologies(text);
            requiredTechs = extracted;
        }

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
            RequiredTechnologies = requiredTechs,
            PreferredTechnologies = preferredTechs,
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

    private static readonly List<(string Pattern, string Display)> TechPatterns =
    [
        // Languages
        ("javascript", "JavaScript"), ("typescript", "TypeScript"), ("python", "Python"),
        ("\\bjava\\b", "Java"), ("csharp", "C#"), ("\\bc#\\b", "C#"), ("\\bgo\\b", "Go"),
        ("golang", "Go"), ("\\brust\\b", "Rust"), ("kotlin", "Kotlin"), ("\\bswift\\b", "Swift"),
        ("\\bruby\\b", "Ruby"), ("\\bphp\\b", "PHP"), ("\\bc\\+\\+", "C++"), ("cpp", "C++"),
        ("scala", "Scala"), ("\\bdart\\b", "Dart"), ("elixir", "Elixir"), ("haskell", "Haskell"),
        ("perl", "Perl"), ("solidity", "Solidity"),
        // Frontend
        ("\\breact\\b", "React"), ("reactjs", "React"), ("vue\\.?js", "Vue.js"),
        ("\\bvue\\b", "Vue.js"), ("angular", "Angular"), ("svelte", "Svelte"),
        ("next\\.?js", "Next.js"), ("nuxt", "Nuxt.js"), ("remix", "Remix"), ("astro", "Astro"),
        ("\\bhtml\\b", "HTML"), ("\\bcss\\b", "CSS"), ("scss", "SCSS"), ("sass", "Sass"),
        ("tailwind", "Tailwind CSS"), ("bootstrap", "Bootstrap"),
        ("material ?ui", "Material UI"), ("chakra", "Chakra UI"),
        ("redux", "Redux"), ("zustand", "Zustand"),
        // Backend
        ("node\\.?js", "Node.js"), ("deno", "Deno"), ("express", "Express.js"),
        ("fastify", "Fastify"), ("nestjs", "NestJS"), ("spring ?boot", "Spring Boot"),
        ("\\bspring\\b", "Spring Boot"), ("django", "Django"), ("flask", "Flask"),
        ("fastapi", "FastAPI"), ("rails", "Ruby on Rails"),
        ("asp\\.?net", "ASP.NET Core"), ("laravel", "Laravel"), ("symfony", "Symfony"),
        ("graphql", "GraphQL"), ("rest api", "REST API"), ("\\bgrpc\\b", "gRPC"),
        ("websocket", "WebSocket"),
        // Databases
        ("postgresql", "PostgreSQL"), ("postgres", "PostgreSQL"), ("mysql", "MySQL"),
        ("sqlite", "SQLite"), ("mongodb", "MongoDB"), ("\\bredis\\b", "Redis"),
        ("elasticsearch", "Elasticsearch"), ("cassandra", "Cassandra"),
        ("dynamodb", "DynamoDB"), ("mariadb", "MariaDB"), ("sql server", "SQL Server"),
        ("oracle", "Oracle"), ("firebase", "Firebase"), ("supabase", "Supabase"),
        // Cloud & DevOps
        ("\\baws\\b", "AWS"), ("amazon web services", "AWS"), ("\\bazure\\b", "Azure"),
        ("\\bgcp\\b", "GCP"), ("google cloud", "GCP"), ("\\bdocker\\b", "Docker"),
        ("kubernetes", "Kubernetes"), ("k8s", "Kubernetes"),
        ("terraform", "Terraform"), ("ansible", "Ansible"),
        ("jenkins", "Jenkins"), ("github actions", "GitHub Actions"),
        ("gitlab ci", "GitLab CI"), ("circleci", "CircleCI"),
        ("prometheus", "Prometheus"), ("grafana", "Grafana"),
        ("datadog", "Datadog"), ("\\bsentry\\b", "Sentry"),
        ("\\bnginx\\b", "Nginx"), ("serverless", "Serverless"),
        // Testing
        ("\\bjest\\b", "Jest"), ("cypress", "Cypress"), ("playwright", "Playwright"),
        ("selenium", "Selenium"), ("pytest", "Pytest"), ("junit", "JUnit"),
        // Mobile
        ("react native", "React Native"), ("flutter", "Flutter"),
        ("\\bandroid\\b", "Android"), ("\\bios\\b", "iOS"), ("expo", "Expo"),
        ("swiftui", "SwiftUI"),
        // Data & AI
        ("machine learning", "Machine Learning"), ("deep learning", "Deep Learning"),
        ("tensorflow", "TensorFlow"), ("pytorch", "PyTorch"),
        ("\\bllm\\b", "LLM"), ("openai", "OpenAI"), ("langchain", "LangChain"),
        ("\\brag\\b", "RAG"), ("\\bnlp\\b", "NLP"), ("computer vision", "Computer Vision"),
        ("data science", "Data Science"), ("data engineering", "Data Engineering"),
        ("apache spark", "Apache Spark"), ("\\bspark\\b", "Apache Spark"),
        ("kafka", "Kafka"), ("airflow", "Airflow"), ("\\bdbt\\b", "dbt"),
        ("snowflake", "Snowflake"), ("databricks", "Databricks"),
        ("bigquery", "BigQuery"), ("tableau", "Tableau"), ("power bi", "Power BI"),
        // Tools
        ("\\bgit\\b", "Git"), ("\\blinux\\b", "Linux"), ("\\bbash\\b", "Bash"),
        ("webpack", "Webpack"), ("\\bvite\\b", "Vite"),
        // Game Dev
        ("unity", "Unity"), ("unreal engine", "Unreal Engine"), ("godot", "Godot"),
        ("three\\.?js", "Three.js"), ("webgl", "WebGL"),
        // Methodologies / Architecture
        ("microservices", "Microservices"), ("oauth", "OAuth"), ("\\bjwt\\b", "JWT"),
        ("saml", "SAML"),
    ];

    private static readonly Regex WordSplitter = new(@"[\s,;:.!?()\[\]{}/\\""'@#$%^&*+=<>|`~–—]+", RegexOptions.Compiled);

    public static (List<string> Required, List<string> Preferred) ExtractTechnologies(string text)
    {
        var required = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return (required, []);

        var lower = text.ToLowerInvariant();
        var found = new HashSet<string>();

        foreach (var (pattern, display) in TechPatterns)
        {
            if (found.Contains(display)) continue;

            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (regex.IsMatch(lower))
            {
                found.Add(display);
                required.Add(display);
            }
        }

        return (required, []);
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
