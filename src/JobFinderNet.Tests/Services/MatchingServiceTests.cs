using Moq;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;

namespace JobFinderNet.Tests.Services;

public class MatchingServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly MatchingService _service;

    public MatchingServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _service = new MatchingService(_mockJobRepo.Object);
    }

    private static Job CreateJob(List<string>? requiredTechs = null, List<string>? preferredTechs = null)
    {
        return new Job
        {
            Id = 1, Title = "Dev", Description = "D", CompanyName = "C", Location = "Remote",
            JobType = "Full-time", Salary = "$100k", SalaryMin = 80000, SalaryMax = 120000,
            ExperienceRequired = "Senior", SeniorityLevel = "Senior", IsRemote = true,
            RequiredTechnologies = requiredTechs ?? ["C#", "React"],
            PreferredTechnologies = preferredTechs ?? ["Docker"],
            EmployerId = "emp1"
        };
    }

    private static UserProfile CreateProfile(List<string>? skills = null, string? seniority = null, string? location = null)
    {
        return new UserProfile
        {
            Id = 1, UserId = "u1",
            Skills = skills ?? ["C#", "React", "Docker"],
            SeniorityLevel = seniority ?? "Senior",
            DesiredSalaryMin = 90000, DesiredSalaryMax = 130000,
            IsOpenToRemote = true, PreferredLocation = location,
            PreferredJobType = "Full-time"
        };
    }

    [Fact]
    public async Task CalculateMatchScore_PerfectMatch_ReturnsHighScore()
    {
        var job = CreateJob();
        var profile = CreateProfile();

        var score = await _service.CalculateMatchScore(job, profile);

        Assert.True(score > 70);
    }

    [Fact]
    public async Task CalculateMatchScore_NoSkills_ReturnsZeroTech()
    {
        var job = CreateJob(requiredTechs: ["Go"], preferredTechs: ["Rust"]);
        var profile = CreateProfile(skills: ["Python", "Java"]);

        var breakdown = await _service.CalculateMatchScoreDetailed(job, profile);

        Assert.Equal(0, breakdown.TechnologyScore);
        Assert.Empty(breakdown.MatchedSkills);
    }

    [Fact]
    public async Task CalculateMatchScore_EmptyProfileSkills_ReturnsZero()
    {
        var job = CreateJob();
        var profile = CreateProfile(skills: []);

        var score = await _service.CalculateMatchScore(job, profile);

        Assert.Equal(0, score);
    }

    [Fact]
    public async Task CalculateMatchScore_NoJobTechs_ReturnsZero()
    {
        var job = CreateJob(requiredTechs: [], preferredTechs: []);
        var profile = CreateProfile();

        var score = await _service.CalculateMatchScore(job, profile);

        Assert.Equal(0, score);
    }

    [Fact]
    public async Task CalculateMatchScoreDetailed_ReturnsBreakdown()
    {
        var job = CreateJob();
        var profile = CreateProfile();

        var breakdown = await _service.CalculateMatchScoreDetailed(job, profile);

        Assert.True(breakdown.TechnologyScore > 0);
        Assert.True(breakdown.SeniorityScore > 0);
        Assert.True(breakdown.SalaryScore > 0);
        Assert.True(breakdown.LocationScore > 0);
        Assert.True(breakdown.JobTypeScore > 0);
        Assert.Equal(breakdown.TechnologyScore + breakdown.SeniorityScore + breakdown.SalaryScore + breakdown.LocationScore + breakdown.JobTypeScore, breakdown.TotalScore);
        Assert.NotEmpty(breakdown.MatchedSkills);
    }

    [Fact]
    public async Task CalculateMatchScore_SeniorityMismatch_ReducesScore()
    {
        var job = CreateJob();
        job.SeniorityLevel = "Junior";
        var profile = CreateProfile(seniority: "Senior");

        var breakdown = await _service.CalculateMatchScoreDetailed(job, profile);

        Assert.True(breakdown.SeniorityScore < 20);
    }

    [Fact]
    public async Task CalculateMatchScore_SalaryMismatch_ReducesScore()
    {
        var job = CreateJob();
        job.SalaryMin = 200000;
        job.SalaryMax = 250000;
        var profile = CreateProfile();

        var breakdown = await _service.CalculateMatchScoreDetailed(job, profile);

        Assert.Equal(0, breakdown.SalaryScore);
    }

    [Fact]
    public async Task CalculateMatchScore_LocationMismatch_ReducesScore()
    {
        var job = CreateJob();
        job.IsRemote = false;
        job.City = "NYC";
        var profile = CreateProfile(location: "LA");

        var breakdown = await _service.CalculateMatchScoreDetailed(job, profile);

        Assert.Equal(0, breakdown.LocationScore);
    }

    [Fact]
    public async Task CalculateMatchScore_JobTypeMismatch_ZeroJobTypeScore()
    {
        var job = CreateJob();
        job.JobType = "Full-time";
        var profile = CreateProfile();
        profile.PreferredJobType = "Internship";

        var breakdown = await _service.CalculateMatchScoreDetailed(job, profile);

        Assert.Equal(0, breakdown.JobTypeScore);
        Assert.Equal("Type mismatch", breakdown.JobTypeMatchReason);
    }

    [Fact]
    public async Task GetTopMatches_ReturnsTopScoredJobs()
    {
        var jobs = new List<Job>
        {
            CreateJob(requiredTechs: ["C#"], preferredTechs: []),
            CreateJob(requiredTechs: ["Python"], preferredTechs: []),
            CreateJob(requiredTechs: ["Go", "Rust"], preferredTechs: [])
        };
        jobs[0].Id = 1; jobs[1].Id = 2; jobs[2].Id = 3;
        _mockJobRepo.Setup(r => r.GetAllActiveJobsAsync()).ReturnsAsync(jobs);

        var profile = CreateProfile(skills: ["C#"]);

        var result = await _service.GetTopMatches(profile, 2);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Score >= result[1].Score);
    }

    [Fact]
    public async Task GetTopMatchesDetailed_ReturnsDetailedResults()
    {
        var jobs = new List<Job> { CreateJob() };
        _mockJobRepo.Setup(r => r.GetAllActiveJobsAsync()).ReturnsAsync(jobs);

        var profile = CreateProfile();

        var result = await _service.GetTopMatchesDetailed(profile, 10);

        Assert.Single(result);
        Assert.NotEmpty(result[0].Breakdown.MatchedSkills);
    }

    [Fact]
    public async Task GetTopMatchesAboveThreshold_FiltersByMinimumScore()
    {
        var jobs = new List<Job>
        {
            CreateJob(requiredTechs: ["C#"], preferredTechs: []),
            CreateJob(requiredTechs: ["Go", "Rust", "Kotlin"], preferredTechs: [])
        };
        jobs[0].Id = 1; jobs[1].Id = 2;
        _mockJobRepo.Setup(r => r.GetAllActiveJobsAsync()).ReturnsAsync(jobs);

        var profile = CreateProfile(skills: ["C#"]);
        profile.MinimumMatchScore = 50;

        var result = await _service.GetTopMatchesAboveThreshold(profile, 10);

        Assert.All(result, r => Assert.True(r.Score >= 50));
    }
}
