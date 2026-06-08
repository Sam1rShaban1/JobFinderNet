using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
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
        var breakdown = await CalculateMatchScoreDetailed(job, profile);
        return breakdown.TotalScore;
    }

    public async Task<MatchScoreBreakdown> CalculateMatchScoreDetailed(Job job, UserProfile profile)
    {
        var breakdown = new MatchScoreBreakdown();

        if (profile.Skills.Count == 0)
            return breakdown;

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
            return breakdown;

        if (requiredTechs.Count > 0 || preferredTechs.Count > 0)
        {
            var requiredMatches = requiredTechs.Intersect(userSkills).ToList();
            var preferredMatches = preferredTechs.Intersect(userSkills).ToList();

            breakdown.MatchedSkills = requiredMatches.Concat(preferredMatches).Distinct().ToList();
            breakdown.MissingSkills = requiredTechs.Except(userSkills).ToList();

            var totalSlots = requiredTechs.Count + preferredTechs.Count * 0.5;
            var matchedSlots = requiredMatches.Count + preferredMatches.Count * 0.5;
            if (totalSlots > 0)
            {
                breakdown.TechnologyScore = (int)Math.Round(matchedSlots / totalSlots * 40);
            }
        }

        // Seniority match (20%)
        if (!string.IsNullOrEmpty(profile.SeniorityLevel))
        {
            if (!string.IsNullOrEmpty(job.SeniorityLevel))
            {
                var levels = new[] { "junior", "mid-level", "senior", "lead", "manager", "director" };
                var userIdx = Array.IndexOf(levels, profile.SeniorityLevel.ToLowerInvariant());
                var jobIdx = Array.IndexOf(levels, job.SeniorityLevel.ToLowerInvariant());

                if (userIdx >= 0 && jobIdx >= 0)
                {
                    var diff = Math.Abs(userIdx - jobIdx);
                    breakdown.SeniorityScore = diff switch
                    {
                        0 => 20,
                        1 => 12,
                        _ => 4,
                    };
                    breakdown.SeniorityMatchReason = diff switch
                    {
                        0 => "Perfect level match",
                        1 => "Close level match",
                        _ => $"Level gap: {profile.SeniorityLevel} vs {job.SeniorityLevel}"
                    };
                }
                else
                {
                    breakdown.SeniorityScore = 10;
                    breakdown.SeniorityMatchReason = "Level not recognized";
                }
            }
            else
            {
                breakdown.SeniorityScore = 10;
                breakdown.SeniorityMatchReason = "Job level not specified";
            }
        }

        // Salary overlap (15%)
        if (profile.DesiredSalaryMin.HasValue || profile.DesiredSalaryMax.HasValue)
        {
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
                    breakdown.SalaryScore = (int)Math.Round((overlapEnd - overlapStart) / userRange * 15);
                }
                else
                {
                    breakdown.SalaryScore = 15;
                }
                breakdown.SalaryMatchReason = "Salary ranges overlap";
            }
            else
            {
                breakdown.SalaryMatchReason = "No salary overlap";
            }
        }

        // Location/Remote (15%)
        if (profile.IsOpenToRemote || !string.IsNullOrEmpty(profile.PreferredLocation))
        {
            if (job.IsRemote && profile.IsOpenToRemote)
            {
                breakdown.LocationScore = 15;
                breakdown.LocationMatchReason = "Remote-friendly";
            }
            else if (!string.IsNullOrEmpty(profile.PreferredLocation) && !string.IsNullOrEmpty(job.City))
            {
                if (profile.PreferredLocation.Equals(job.City, StringComparison.OrdinalIgnoreCase))
                {
                    breakdown.LocationScore = 15;
                    breakdown.LocationMatchReason = $"Exact city match: {job.City}";
                }
                else if (!string.IsNullOrEmpty(job.State) &&
                         profile.PreferredLocation.Equals(job.State, StringComparison.OrdinalIgnoreCase))
                {
                    breakdown.LocationScore = 10;
                    breakdown.LocationMatchReason = $"State match: {job.State}";
                }
                else if (!string.IsNullOrEmpty(job.Country) &&
                         profile.PreferredLocation.Equals(job.Country, StringComparison.OrdinalIgnoreCase))
                {
                    breakdown.LocationScore = 5;
                    breakdown.LocationMatchReason = $"Country match: {job.Country}";
                }
                else
                {
                    breakdown.LocationMatchReason = "Location mismatch";
                }
            }
        }

        // Job type (10%)
        if (!string.IsNullOrEmpty(profile.PreferredJobType))
        {
            if (!string.IsNullOrEmpty(job.JobType))
            {
                if (profile.PreferredJobType.Equals(job.JobType, StringComparison.OrdinalIgnoreCase))
                {
                    breakdown.JobTypeScore = 10;
                    breakdown.JobTypeMatchReason = "Exact type match";
                }
                else if (IsCompatibleJobType(profile.PreferredJobType, job.JobType))
                {
                    breakdown.JobTypeScore = 7;
                    breakdown.JobTypeMatchReason = "Compatible type";
                }
                else
                {
                    breakdown.JobTypeMatchReason = "Type mismatch";
                }
            }
            else
            {
                breakdown.JobTypeScore = 5;
                breakdown.JobTypeMatchReason = "Job type not specified";
            }
        }

        breakdown.TotalScore = breakdown.TechnologyScore + breakdown.SeniorityScore
            + breakdown.SalaryScore + breakdown.LocationScore + breakdown.JobTypeScore;

        return breakdown;
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

    public async Task<List<MatchedJobDto>> GetTopMatchesDetailed(UserProfile profile, int limit = 10)
    {
        var jobs = await _context.Jobs
            .Where(j => j.IsActive)
            .AsNoTracking()
            .ToListAsync();

        var results = new List<MatchedJobDto>();

        foreach (var job in jobs)
        {
            var breakdown = await CalculateMatchScoreDetailed(job, profile);
            if (breakdown.TotalScore > 0)
            {
                results.Add(new MatchedJobDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = job.CompanyName,
                    Location = job.Location,
                    JobType = job.JobType,
                    Salary = job.Salary,
                    ExperienceRequired = job.ExperienceRequired,
                    PostedDate = job.PostedDate,
                    IsRemote = job.IsRemote,
                    Score = breakdown.TotalScore,
                    Breakdown = breakdown
                });
            }
        }

        return results.OrderByDescending(r => r.Score).Take(limit).ToList();
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
