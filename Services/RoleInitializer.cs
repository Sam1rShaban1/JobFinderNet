using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using JobFinderNet.Models;
using JobFinderNet.Factories;

namespace JobFinderNet.Services;

public class RoleInitializer
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleInitializer(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "Employer", "Applicant" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create default admin user
        var adminEmail = "admin@jobfinder.net";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = UserFactory.CreateEmployer(adminEmail, "JobFinder Admin");
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Create default employer user
        var employerEmail = "employer@jobfinder.net";
        var employerUser = await userManager.FindByEmailAsync(employerEmail);

        if (employerUser == null)
        {
            employerUser = UserFactory.CreateEmployer(employerEmail, "Demo Company");
            var result = await userManager.CreateAsync(employerUser, "Employer123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employerUser, "Employer");
            }
        }

        // Create default applicant user
        var applicantEmail = "applicant@jobfinder.net";
        var applicantUser = await userManager.FindByEmailAsync(applicantEmail);

        if (applicantUser == null)
        {
            applicantUser = UserFactory.CreateApplicant(applicantEmail);
            var result = await userManager.CreateAsync(applicantUser, "Applicant123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(applicantUser, "Applicant");
            }
        }
    }
} 