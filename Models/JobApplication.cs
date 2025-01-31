using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobFinderNet.Data;

public class JobApplication
{
    public int Id { get; set; }
    
    [Required]
    public string ApplicantId { get; set; }
    
    [ForeignKey("ApplicantId")]
    public ApplicationUser? Applicant { get; set; }
    
    [Required]
    public int JobId { get; set; }
    
    [ForeignKey("JobId")]
    public Job? Job { get; set; }
    
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
}

public enum ApplicationStatus { Pending, Approved, Rejected }