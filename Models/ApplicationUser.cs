using Microsoft.AspNetCore.Identity;

namespace JobFinderNet.Data;

public class ApplicationUser : IdentityUser
{
    public string? CompanyName { get; set; }
    // Add any other custom properties here
}

