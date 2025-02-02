using Microsoft.AspNetCore.Identity;

namespace JobFinderNet.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    // Add any other custom properties here
}

