using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Starting data seeding...");

            var employers = new List<(string Email, string Name)>
            {
                ("employer1@test.com", "Tech Corp"),
                ("employer2@test.com", "Digital Solutions"),
                ("employer3@test.com", "Innovation Labs")
            };

            var employerIds = new List<string>();

            foreach (var (email, name) in employers)
            {
                var employer = await userManager.FindByEmailAsync(email);
                if (employer == null)
                {
                    employer = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        CompanyName = name
                    };

                    var result = await userManager.CreateAsync(employer, "Employer123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(employer, "Employer");
                        employerIds.Add(employer.Id);
                        logger.LogInformation("Created employer: {Email}", email);
                    }
                }
                else
                {
                    employerIds.Add(employer.Id);
                }
            }

            logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during data seeding");
        }
    }

    public static async Task<string> GetOrCreateSystemEmployer(UserManager<ApplicationUser> userManager)
    {
        const string email = "jobs@jobfinder.net";
        var employer = await userManager.FindByEmailAsync(email);
        if (employer == null)
        {
            employer = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CompanyName = "JobFinder Aggregator"
            };
            var result = await userManager.CreateAsync(employer, Guid.NewGuid().ToString("N") + "!Aa1");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employer, "Employer");
            }
        }
        return employer.Id;
    }
}
