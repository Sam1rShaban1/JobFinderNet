using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Tests.Helpers;

public class MockAiService : IAiService
{
    public Task<ParsedResume> ParseResumeAsync(ParseResumeRequest request)
    {
        return Task.FromResult(new ParsedResume
        {
            Skills = ["C#", "JavaScript", "React", "Node.js", "PostgreSQL"],
            SeniorityLevel = "Senior",
            ExperienceYears = 5,
            Education =
            [
                new ResumeEducation { Degree = "B.S. Computer Science", Institution = "MIT", Year = "2019" }
            ],
            Summary = "Senior software engineer with 5 years of experience in full-stack development.",
            JobTitles = ["Software Engineer", "Full Stack Developer"]
        });
    }

    public Task<List<MatchedJobDto>> GetRecommendationsAsync(string userId, ParsedResume resume, int limit = 10)
    {
        return Task.FromResult(new List<MatchedJobDto>
        {
            new()
            {
                Id = 1,
                Title = "Senior Software Engineer",
                CompanyName = "Tech Corp",
                Location = "Remote",
                JobType = "Full-time",
                Salary = "$120,000/year",
                ExperienceRequired = "3-5 years",
                PostedDate = DateTime.UtcNow,
                IsRemote = true,
                Score = 85,
                Breakdown = new MatchScoreBreakdown
                {
                    TechnologyScore = 35,
                    SeniorityScore = 18,
                    SalaryScore = 12,
                    LocationScore = 10,
                    JobTypeScore = 10,
                    TotalScore = 85
                }
            }
        });
    }

    public Task<CoverLetterResponse> GenerateCoverLetterAsync(string userId, CoverLetterRequest request)
    {
        return Task.FromResult(new CoverLetterResponse
        {
            CoverLetter = $"Dear Hiring Manager,\n\nI am writing to express my interest in the {request.JobTitle} position at {request.CompanyName}. With my experience in software engineering, I believe I would be a strong fit for this role.\n\nBest regards",
            Tips = "1. Add specific metrics\n2. Research the company\n3. Customize for the role"
        });
    }
}
