using System.Text.Json.Serialization;

namespace JobFinderNet.Core.Models;

public class Application
{
    public int Id { get; set; }
    public DateTime AppliedDate { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public required int JobId { get; set; }
    public required string ApplicantId { get; set; }

    public required Job Job { get; set; }
    public required ApplicationUser Applicant { get; set; }

    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }

    public ICollection<ApplicationNote> Notes { get; set; } = new List<ApplicationNote>();
}

public class ApplicationNote
{
    public int Id { get; set; }
    public required int ApplicationId { get; set; }
    public required string UserId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonIgnore]
    public Application Application { get; set; } = null!;
}
