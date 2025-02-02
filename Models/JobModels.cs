using System;
using System.Collections.Generic;
using JobFinderNet.Data;

namespace JobFinderNet.Models;

public class Job
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string CompanyName { get; set; }
    public required string Location { get; set; }
    public required string JobType { get; set; }
    public required string Salary { get; set; }
    public required string ExperienceRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    
    public required string EmployerId { get; set; }
    public required ApplicationUser Employer { get; set; }
    
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}

public enum ApplicationStatus
{
    Pending,
    Accepted,
    Rejected
} 