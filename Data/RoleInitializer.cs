using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobFinderNet.Data;

public static class RoleInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

        string[] roles = { "Admin", "Employer", "Applicant" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                logger.LogInformation($"Creating role: {role}");
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    logger.LogError($"Failed to create role {role}: {string.Join(", ", result.Errors)}");
                }
            }
        }
    }
} 