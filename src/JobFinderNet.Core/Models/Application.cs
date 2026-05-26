namespace JobFinderNet.Core.Models;

public class Application
{
    public int Id { get; set; }
    public DateTime AppliedDate { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public required int JobId { get; set; }
    public required string ApplicantId { get; set; }

    public required Job Job { get; set; }
    public required ApplicationUser Applicant { get; set; }

    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }
}
