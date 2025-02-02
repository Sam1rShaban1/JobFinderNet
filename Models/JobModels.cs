using System;
using System.Collections.Generic;

namespace JobFinderNet.Models;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Company { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    
    public string EmployerId { get; set; }
    public ApplicationUser Employer { get; set; }
    
    public ICollection<JobApplication> Applications { get; set; }
}

public class JobApplication
{
    public int Id { get; set; }
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public ApplicationStatus Status { get; set; }
    
    public int JobId { get; set; }
    public Job Job { get; set; }
    
    public string ApplicantId { get; set; }
    public ApplicationUser Applicant { get; set; }
}

public enum ApplicationStatus
{
    Pending,
    Accepted,
    Rejected
} 