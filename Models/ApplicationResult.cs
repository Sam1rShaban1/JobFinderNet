using JobFinderNet.Models;

namespace JobFinderNet.Models;

public class ApplicationResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public JobApplication? Application { get; private set; }

    public static ApplicationResult CreateSuccess(JobApplication application) =>
        new() { Success = true, Application = application };

    public static ApplicationResult CreateError(string error) =>
        new() { Success = false, Error = error };
} 