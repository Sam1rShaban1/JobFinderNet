using Bogus;
using JobFinderNet.Data;

namespace JobFinderNet.Factories;

public static class UserFactory
{
    public static Faker<ApplicationUser> CreateEmployer()
    {
        return new Faker<ApplicationUser>()
            .RuleFor(u => u.UserName, f => f.Internet.Email())
            .RuleFor(u => u.Email, (f, u) => u.UserName)
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());
    }

    public static Faker<ApplicationUser> CreateJobSeeker()
    {
        return new Faker<ApplicationUser>()
            .RuleFor(u => u.UserName, f => f.Internet.Email())
            .RuleFor(u => u.Email, (f, u) => u.UserName)
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());
    }
} 