using System;
using JobFinderNet.Data;

namespace JobFinderNet.Models
{
    public class Application
    {
        public int Id { get; set; }
        public DateTime AppliedDate { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        
        // Foreign keys
        public required int JobId { get; set; }
        public required string ApplicantId { get; set; }
        
        // Navigation properties
        public required Job Job { get; set; }
        public required ApplicationUser Applicant { get; set; }
        
        // Optional fields should be nullable
        public string? CoverLetter { get; set; }
        public string? ResumeUrl { get; set; }
    }
} 