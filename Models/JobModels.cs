using System;
using System.Collections.Generic;

namespace JobFinderNet.Models;

public class Job
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Company { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    
    public required string EmployerId { get; set; }
    public required JobFinderNet.Data.ApplicationUser Employer { get; set; }
    
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}

public class JobApplication
{
    public int Id { get; set; }
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public ApplicationStatus Status { get; set; }
    
    public int JobId { get; set; }
    public required Job Job { get; set; }
    
    public required string ApplicantId { get; set; }
    public required JobFinderNet.Data.ApplicationUser Applicant { get; set; }
}

public enum ApplicationStatus
{
    Pending,
    Accepted,
    Rejected
} 