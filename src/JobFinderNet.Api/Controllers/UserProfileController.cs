using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMatchingService _matchingService;

    public ProfileController(ApplicationDbContext context, IMatchingService matchingService)
    {
        _context = context;
        _matchingService = matchingService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfile>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile != null)
            return Ok(profile);

        profile = new UserProfile
        {
            UserId = userId,
            Skills = [],
            EmailFrequency = "daily",
            MinimumMatchScore = 20,
            IsOpenToRemote = true,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.UserProfiles.Add(profile);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<UserProfile>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            _context.UserProfiles.Add(profile);
        }

        profile.Skills = dto.Skills ?? profile.Skills;
        profile.SeniorityLevel = dto.SeniorityLevel;
        profile.DesiredSalaryMin = dto.DesiredSalaryMin;
        profile.DesiredSalaryMax = dto.DesiredSalaryMax;
        profile.IsOpenToRemote = dto.IsOpenToRemote;
        profile.PreferredLocation = dto.PreferredLocation;
        profile.PreferredJobType = dto.PreferredJobType;
        profile.EmailOnMatch = dto.EmailOnMatch;
        profile.MinimumMatchScore = dto.MinimumMatchScore;
        profile.EmailFrequency = dto.EmailFrequency;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(profile);
    }

    [HttpGet("matched")]
    public async Task<ActionResult> GetMatchedJobs([FromQuery] int limit = 6)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null || profile.Skills.Count == 0)
            return Ok(new List<object>());

        var matches = await _matchingService.GetTopMatches(profile, limit);
        var result = matches.Select(m => new
        {
            m.Job.Id,
            m.Job.Title,
            m.Job.CompanyName,
            m.Job.Location,
            m.Job.JobType,
            m.Job.Salary,
            m.Job.ExperienceRequired,
            m.Job.PostedDate,
            m.Job.IsRemote,
            Score = m.Score,
        });

        return Ok(result);
    }

    [HttpGet("skills")]
    public ActionResult<List<string>> GetAvailableSkills()
    {
        var skills = new List<string>
        {
            // Languages
            "JavaScript", "TypeScript", "Python", "Java", "C#", "Go", "Rust", "Kotlin", "Swift",
            "Ruby", "PHP", "C++", "C", "Scala", "R", "Dart", "Elixir", "Clojure", "Haskell",
            "Perl", "Lua", "Zig", "Solidity",
            // Frontend
            "React", "Vue.js", "Angular", "Svelte", "Next.js", "Nuxt.js", "Remix", "Astro",
            "HTML", "CSS", "SCSS", "Tailwind CSS", "Bootstrap", "Material UI", "Chakra UI",
            "Redux", "Zustand", "Recoil", "Jotai", "TanStack Query",
            // Backend
            "Node.js", "Deno", "Express.js", "Fastify", "NestJS", "Spring Boot", "Django",
            "Flask", "FastAPI", "Ruby on Rails", "ASP.NET Core", "Laravel", "Symfony",
            "GraphQL", "REST API", "gRPC", "WebSocket",
            // Databases
            "PostgreSQL", "MySQL", "SQLite", "MongoDB", "Redis", "Elasticsearch", "Cassandra",
            "DynamoDB", "MariaDB", "SQL Server", "Oracle", "Firebase", "Supabase", "Neon",
            "PlanetScale", "CockroachDB", "ClickHouse", "InfluxDB", "Neo4j",
            // Cloud & DevOps
            "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Terraform", "Ansible", "Pulumi",
            "CI/CD", "GitHub Actions", "GitLab CI", "Jenkins", "CircleCI", "ArgoCD",
            "Helm", "Prometheus", "Grafana", "Datadog", "New Relic", "Sentry",
            "Nginx", "Traefik", "Cloudflare", "Vercel", "Netlify", "Railway",
            // Testing
            "Jest", "Vitest", "Cypress", "Playwright", "Selenium", "Pytest", "JUnit",
            "Mocha", "Chai", "Testing Library", "Storybook", "MSW",
            // Mobile
            "React Native", "Flutter", "Android", "iOS", "Expo", "Xamarin", "SwiftUI",
            // Data & AI
            "Machine Learning", "Deep Learning", "TensorFlow", "PyTorch", "LLM", "OpenAI",
            "LangChain", "RAG", "NLP", "Computer Vision", "Data Science", "Data Engineering",
            "Apache Spark", "Kafka", "Airflow", "dbt", "Snowflake", "Databricks", "BigQuery",
            "Tableau", "Power BI", "Looker",
            // Tools & Platforms
            "Git", "Linux", "Bash", "Make", "Webpack", "Vite", "esbuild", "Rollup",
            "Yarn", "pnpm", "npm", "nx", "turborepo", "Monorepo",
            "Jira", "Confluence", "Notion", "Linear", "Slack",
            // Security
            "Cybersecurity", "Penetration Testing", "OAuth", "SAML", "JWT", "Zero Trust",
            "Cryptography", "SIEM", "SOC 2", "HIPAA", "GDPR",
            // Methodologies
            "Agile", "Scrum", "Kanban", "TDD", "DDD", "Microservices", "Event-Driven",
            "Serverless", "Edge Computing", "WebAssembly",
            // Game Dev
            "Unity", "Unreal Engine", "Godot", "WebGL", "Three.js", "WebGPU",
        };

        return Ok(skills);
    }
}

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
