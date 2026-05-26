using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JobFinderNet.Core.Models;
using JobFinderNet.Infrastructure.Data;
using JobFinderNet.Infrastructure.Factories;

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
                        logger.LogInformation($"Created employer: {email}");
                    }
                }
                else
                {
                    employerIds.Add(employer.Id);
                }
            }

            if (!await context.Jobs.AnyAsync())
            {
                var seedEmployers = await context.Users
                    .Where(u => employerIds.Contains(u.Id))
                    .Cast<ApplicationUser>()
                    .ToListAsync();

                var jobFaker = JobFactory.Create(employerIds, context);
                var jobs = jobFaker.Generate(40);
                await context.Jobs.AddRangeAsync(jobs);
                await context.SaveChangesAsync();
            }

            var seekers = UserFactory.CreateJobSeeker().Generate(10);
            var seekerIds = new List<string>();

            foreach (var seeker in seekers)
            {
                if (await userManager.FindByEmailAsync(seeker.Email!) == null)
                {
                    var result = await userManager.CreateAsync(seeker, "Seeker123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(seeker, "Applicant");
                        seekerIds.Add(seeker.Id);
                    }
                }
            }

            if (!context.Applications.Any() && seekerIds.Any())
            {
                var jobs = await context.Jobs
                    .Include(j => j.Employer)
                    .ToListAsync();

                var applicants = await context.Users
                    .Where(u => seekerIds.Contains(u.Id))
                    .Cast<ApplicationUser>()
                    .ToListAsync();

                var applications = new List<Application>();
                var random = new Random();

                foreach (var applicant in applicants)
                {
                    var numApplications = random.Next(1, 4);
                    var selectedJobs = jobs.OrderBy(x => random.Next()).Take(numApplications);

                    foreach (var job in selectedJobs)
                    {
                        var application = new Application
                        {
                            AppliedDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-random.Next(1, 30)), DateTimeKind.Utc),
                            Status = ApplicationStatus.Pending,
                            JobId = job.Id,
                            Job = job,
                            ApplicantId = applicant.Id,
                            Applicant = applicant
                        };
                        applications.Add(application);
                    }
                }

                await context.Applications.AddRangeAsync(applications);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during data seeding");
            throw;
        }
    }
}
