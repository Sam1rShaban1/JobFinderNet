namespace JobFinderNet.Core.Models;

public class SavedJob
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int JobId { get; set; }
    public DateTime SavedDate { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonIgnore]
    public Job Job { get; set; } = null!;
}
