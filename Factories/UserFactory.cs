using JobFinderNet.Models;
using Microsoft.AspNetCore.Identity;
using Bogus;

namespace JobFinderNet.Factories;

public static class UserFactory
{
    public static ApplicationUser CreateApplicant(string email)
    {
        return new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
    }

    public static ApplicationUser CreateEmployer(string email, string companyName)
    {
        return new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            CompanyName = companyName
        };
    }

    public static Faker<ApplicationUser> CreateEmployer()
    {
        return new Faker<ApplicationUser>()
            .RuleFor(u => u.UserName, f => f.Internet.Email())
            .RuleFor(u => u.Email, (f, u) => u.UserName)
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.CompanyName, f => f.Company.CompanyName())
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());
    }

    public static Faker<ApplicationUser> CreateJobSeeker()
    {
        return new Faker<ApplicationUser>()
            .RuleFor(u => u.UserName, f => f.Internet.Email())
            .RuleFor(u => u.Email, (f, u) => u.UserName)
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());
    }
} 