using Bogus;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobFinderNet.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFinderNet.Data;

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

            // Create employer users if they don't exist
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

            // Seed jobs if none exist
            if (!await context.Jobs.AnyAsync())
            {
                var jobFaker = JobFactory.Create(employerIds, context);
                var jobs = jobFaker.Generate(10);
                await context.Jobs.AddRangeAsync(jobs);
                await context.SaveChangesAsync();
            }

            // Create job seekers
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

            // Create applications
            if (!context.Applications.Any() && seekerIds.Any())
            {
                // Get jobs with their employers
                var jobs = await context.Jobs
                    .Include(j => j.Employer)
                    .ToListAsync();

                // Get applicants
                var applicants = await context.Users
                    .Where(u => seekerIds.Contains(u.Id))
                    .ToListAsync();

                var applications = new List<JobApplication>();
                var random = new Random();

                foreach (var applicant in applicants)
                {
                    var numApplications = random.Next(1, 4); // 1-3 applications per seeker
                    var selectedJobs = jobs.OrderBy(x => random.Next()).Take(numApplications);

                    foreach (var job in selectedJobs)
                    {
                        var application = new JobApplication
                        {
                            AppliedDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-random.Next(1, 30)), DateTimeKind.Utc),
                            Status = (ApplicationStatus)random.Next(0, 3),
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