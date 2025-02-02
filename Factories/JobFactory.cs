using Bogus;
using JobFinderNet.Models;
using JobFinderNet.Data;

namespace JobFinderNet.Factories;

public static class JobFactory
{
    public static Faker<Job> Create(List<string> employerIds, ApplicationDbContext context)
    {
        return new Faker<Job>()
            .RuleFor(j => j.Title, f => f.Name.JobTitle())
            .RuleFor(j => j.Description, f => f.Lorem.Paragraphs(2))
            .RuleFor(j => j.CompanyName, f => f.Company.CompanyName())
            .RuleFor(j => j.Location, f => f.Address.City())
            .RuleFor(j => j.JobType, f => f.PickRandom(new[] { "Full-time", "Part-time", "Contract", "Internship" }))
            .RuleFor(j => j.Salary, f => $"${f.Random.Number(30000, 150000)}/year")
            .RuleFor(j => j.ExperienceRequired, f => f.PickRandom(new[] { "Entry Level", "1-3 years", "3-5 years", "5+ years" }))
            .RuleFor(j => j.IsActive, true)
            .RuleFor(j => j.PostedDate, f => DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc))
            .RuleFor(j => j.EmployerId, f => f.PickRandom(employerIds))
            .RuleFor(j => j.Employer, (f, j) => context.Users.Find(j.EmployerId));
    }

    public static Job CreateJob(string title, string description, string companyName, string employerId, ApplicationUser employer)
    {
        return new Job
        {
            Title = title,
            Description = description,
            CompanyName = companyName,
            Location = "Remote",
            JobType = "Full-time",
            Salary = "Competitive",
            ExperienceRequired = "Entry Level",
            IsActive = true,
            PostedDate = DateTime.UtcNow,
            EmployerId = employerId,
            Employer = employer
        };
    }
} 