using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _context;

    public MatchingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CalculateMatchScore(Job job, UserProfile profile)
    {
        if (profile.Skills.Count == 0)
            return 0;

        var earned = 0.0;
        var maxPossible = 0.0;

        // Technology overlap (40%)
        var requiredTechs = job.RequiredTechnologies
            .Select(t => t.ToLowerInvariant())
            .ToList();
        var preferredTechs = job.PreferredTechnologies
            .Select(t => t.ToLowerInvariant())
            .ToList();
        var userSkills = profile.Skills
            .Select(s => s.ToLowerInvariant())
            .ToList();

        if (requiredTechs.Count == 0 && preferredTechs.Count == 0)
            return 0;

        if (requiredTechs.Count > 0 || preferredTechs.Count > 0)
        {
            var requiredMatches = requiredTechs.Intersect(userSkills).Count();
            var preferredMatches = preferredTechs.Intersect(userSkills).Count();
            var totalSlots = requiredTechs.Count + preferredTechs.Count * 0.5;
            var matchedSlots = requiredMatches + preferredMatches * 0.5;
            if (totalSlots > 0)
            {
                earned += matchedSlots / totalSlots * 40;
                maxPossible += 40;
            }
        }

        // Seniority match (20%)
        if (!string.IsNullOrEmpty(profile.SeniorityLevel))
        {
            maxPossible += 20;

            if (!string.IsNullOrEmpty(job.SeniorityLevel))
            {
                var levels = new[] { "junior", "mid-level", "senior", "lead", "manager", "director" };
                var userIdx = Array.IndexOf(levels, profile.SeniorityLevel.ToLowerInvariant());
                var jobIdx = Array.IndexOf(levels, job.SeniorityLevel.ToLowerInvariant());

                if (userIdx >= 0 && jobIdx >= 0)
                {
                    var diff = Math.Abs(userIdx - jobIdx);
                    earned += diff switch
                    {
                        0 => 20,
                        1 => 12,
                        _ => 4,
                    };
                }
                else
                {
                    earned += 10;
                }
            }
            else
            {
                earned += 10;
            }
        }

        // Salary overlap (15%)
        if (profile.DesiredSalaryMin.HasValue || profile.DesiredSalaryMax.HasValue)
        {
            maxPossible += 15;

            var userMin = profile.DesiredSalaryMin ?? 0;
            var userMax = profile.DesiredSalaryMax ?? double.MaxValue;
            var jobMin = job.SalaryMin ?? 0;
            var jobMax = job.SalaryMax ?? double.MaxValue;

            var overlapStart = Math.Max(userMin, jobMin);
            var overlapEnd = Math.Min(userMax, jobMax);

            if (overlapEnd >= overlapStart)
            {
                var userRange = userMax - userMin;
                if (userRange > 0)
                {
                    earned += (overlapEnd - overlapStart) / userRange * 15;
                }
                else
                {
                    earned += 15;
                }
            }
        }

        // Location/Remote (15%)
        if (profile.IsOpenToRemote || !string.IsNullOrEmpty(profile.PreferredLocation))
        {
            maxPossible += 15;

            if (job.IsRemote && profile.IsOpenToRemote)
            {
                earned += 15;
            }
            else if (!string.IsNullOrEmpty(profile.PreferredLocation) && !string.IsNullOrEmpty(job.City))
            {
                if (profile.PreferredLocation.Equals(job.City, StringComparison.OrdinalIgnoreCase))
                {
                    earned += 15;
                }
                else if (!string.IsNullOrEmpty(job.State) &&
                         profile.PreferredLocation.Equals(job.State, StringComparison.OrdinalIgnoreCase))
                {
                    earned += 10;
                }
                else if (!string.IsNullOrEmpty(job.Country) &&
                         profile.PreferredLocation.Equals(job.Country, StringComparison.OrdinalIgnoreCase))
                {
                    earned += 5;
                }
            }
        }

        // Job type (10%)
        if (!string.IsNullOrEmpty(profile.PreferredJobType))
        {
            maxPossible += 10;

            if (!string.IsNullOrEmpty(job.JobType))
            {
                if (profile.PreferredJobType.Equals(job.JobType, StringComparison.OrdinalIgnoreCase))
                {
                    earned += 10;
                }
                else if (IsCompatibleJobType(profile.PreferredJobType, job.JobType))
                {
                    earned += 7;
                }
            }
            else
            {
                earned += 5;
            }
        }

        if (maxPossible == 0) return 0;

        return (int)Math.Round(earned / maxPossible * 100);
    }

    public async Task<List<(Job Job, int Score)>> GetTopMatches(UserProfile profile, int limit = 10)
    {
        var jobs = await _context.Jobs
            .Where(j => j.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var scored = new List<(Job Job, int Score)>();

        foreach (var job in jobs)
        {
            var score = await CalculateMatchScore(job, profile);
            scored.Add((job, score));
        }

        return scored.OrderByDescending(s => s.Score).Take(limit).ToList();
    }

    public async Task<List<(Job Job, int Score)>> GetTopMatchesAboveThreshold(UserProfile profile, int limit = 10)
    {
        var jobs = await _context.Jobs
            .Where(j => j.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var scored = new List<(Job Job, int Score)>();

        foreach (var job in jobs)
        {
            var score = await CalculateMatchScore(job, profile);
            if (score >= profile.MinimumMatchScore)
            {
                scored.Add((job, score));
            }
        }

        return scored.OrderByDescending(s => s.Score).Take(limit).ToList();
    }

    private static bool IsCompatibleJobType(string preferred, string jobType)
    {
        var p = preferred.ToLowerInvariant();
        var j = jobType.ToLowerInvariant();

        return (p == "full-time" && j == "part-time") ||
               (p == "part-time" && j == "full-time") ||
               (p == "contract" && j == "full-time") ||
               (p == "full-time" && j == "contract");
    }
}
