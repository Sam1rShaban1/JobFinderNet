using Bogus;
using Microsoft.AspNetCore.Identity;
using JobFinderNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobFinderNet.Factories;

namespace JobFinderNet.Data;

public class DataSeeder
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

        try
        {
            logger.LogInformation("Starting data seeding...");

            // Create employers
            var employers = UserFactory.CreateEmployer().Generate(5);
            var employerIds = new List<string>();

            foreach (var employer in employers)
            {
                if (await userManager.FindByEmailAsync(employer.Email!) == null)
                {
                    var result = await userManager.CreateAsync(employer, "Employer123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(employer, "Employer");
                        employerIds.Add(employer.Id);
                        logger.LogInformation($"Created employer: {employer.Email}");
                    }
                }
            }

            // Create jobs
            if (!context.Jobs.Any() && employerIds.Any())
            {
                var jobs = JobFactory.Create(employerIds).Generate(20);
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