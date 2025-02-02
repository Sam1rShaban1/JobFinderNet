using Bogus;
using JobFinderNet.Models;

namespace JobFinderNet.Factories;

public static class JobFactory
{
    public static Faker<Job> Create(List<string> employerIds)
    {
        return new Faker<Job>()
            .RuleFor(j => j.Title, f => f.Name.JobTitle())
            .RuleFor(j => j.Description, f => f.Lorem.Paragraphs(2))
            .RuleFor(j => j.Company, f => f.Company.CompanyName())
            .RuleFor(j => j.IsActive, true)
            .RuleFor(j => j.PostedDate, f => DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc))
            .RuleFor(j => j.EmployerId, f => f.PickRandom(employerIds));
    }
} 