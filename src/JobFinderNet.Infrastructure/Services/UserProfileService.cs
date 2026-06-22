using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMatchingService _matchingService;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IMatchingService matchingService)
    {
        _userProfileRepository = userProfileRepository;
        _matchingService = matchingService;
    }

    public async Task<UserProfile> GetOrCreateProfileAsync(string userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile != null) return profile;

        profile = new UserProfile
        {
            UserId = userId,
            Skills = [],
            EmailFrequency = "daily",
            MinimumMatchScore = 20,
            IsOpenToRemote = true,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            await _userProfileRepository.AddAsync(profile);
            await _userProfileRepository.SaveChangesAsync();
        }
        catch
        {
            profile = await _userProfileRepository.GetByUserIdAsync(userId);
        }

        return profile!;
    }

    public async Task<UserProfile> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            await _userProfileRepository.AddAsync(profile);
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

        await _userProfileRepository.SaveChangesAsync();
        return profile;
    }

    public async Task<List<object>> GetMatchedJobsAsync(string userId, int limit = 6)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Skills.Count == 0)
            return new List<object>();

        var matches = await _matchingService.GetTopMatches(profile, limit);
        return matches.Select(m => (object)new
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
        }).ToList();
    }

    public async Task<List<MatchedJobDto>> GetMatchedJobsDetailedAsync(string userId, int limit = 12)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Skills.Count == 0)
            return new List<MatchedJobDto>();

        return await _matchingService.GetTopMatchesDetailed(profile, limit);
    }

    public Task<List<string>> GetAvailableSkillsAsync()
    {
        var skills = new List<string>
        {
            "JavaScript", "TypeScript", "Python", "Java", "C#", "Go", "Rust", "Kotlin", "Swift",
            "Ruby", "PHP", "C++", "C", "Scala", "R", "Dart", "Elixir", "Clojure", "Haskell",
            "Perl", "Lua", "Zig", "Solidity",
            "React", "Vue.js", "Angular", "Svelte", "Next.js", "Nuxt.js", "Remix", "Astro",
            "HTML", "CSS", "SCSS", "Tailwind CSS", "Bootstrap", "Material UI", "Chakra UI",
            "Redux", "Zustand", "Recoil", "Jotai", "TanStack Query",
            "Node.js", "Deno", "Express.js", "Fastify", "NestJS", "Spring Boot", "Django",
            "Flask", "FastAPI", "Ruby on Rails", "ASP.NET Core", "Laravel", "Symfony",
            "GraphQL", "REST API", "gRPC", "WebSocket",
            "PostgreSQL", "MySQL", "SQLite", "MongoDB", "Redis", "Elasticsearch", "Cassandra",
            "DynamoDB", "MariaDB", "SQL Server", "Oracle", "Firebase", "Supabase", "Neon",
            "PlanetScale", "CockroachDB", "ClickHouse", "InfluxDB", "Neo4j",
            "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Terraform", "Ansible", "Pulumi",
            "CI/CD", "GitHub Actions", "GitLab CI", "Jenkins", "CircleCI", "ArgoCD",
            "Helm", "Prometheus", "Grafana", "Datadog", "New Relic", "Sentry",
            "Nginx", "Traefik", "Cloudflare", "Vercel", "Netlify", "Railway",
            "Jest", "Vitest", "Cypress", "Playwright", "Selenium", "Pytest", "JUnit",
            "Mocha", "Chai", "Testing Library", "Storybook", "MSW",
            "React Native", "Flutter", "Android", "iOS", "Expo", "Xamarin", "SwiftUI",
            "Machine Learning", "Deep Learning", "TensorFlow", "PyTorch", "LLM", "OpenAI",
            "LangChain", "RAG", "NLP", "Computer Vision", "Data Science", "Data Engineering",
            "Apache Spark", "Kafka", "Airflow", "dbt", "Snowflake", "Databricks", "BigQuery",
            "Tableau", "Power BI", "Looker",
            "Git", "Linux", "Bash", "Make", "Webpack", "Vite", "esbuild", "Rollup",
            "Yarn", "pnpm", "npm", "nx", "turborepo", "Monorepo",
            "Jira", "Confluence", "Notion", "Linear", "Slack",
            "Cybersecurity", "Penetration Testing", "OAuth", "SAML", "JWT", "Zero Trust",
            "Cryptography", "SIEM", "SOC 2", "HIPAA", "GDPR",
            "Agile", "Scrum", "Kanban", "TDD", "DDD", "Microservices", "Event-Driven",
            "Serverless", "Edge Computing", "WebAssembly",
            "Unity", "Unreal Engine", "Godot", "WebGL", "Three.js", "WebGPU",
        };

        return Task.FromResult(skills);
    }
}
