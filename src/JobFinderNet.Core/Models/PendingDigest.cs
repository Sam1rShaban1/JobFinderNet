namespace JobFinderNet.Core.Models;

public class PendingDigest
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required int JobId { get; set; }
    public int MatchScore { get; set; }
    public required string JobTitle { get; set; }
    public required string CompanyName { get; set; }
    public string? Location { get; set; }
    public string? Salary { get; set; }
    public required string EmailFrequency { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = null!;
}
