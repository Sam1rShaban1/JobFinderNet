public class ApplicationResult
{
    public bool Success { get; }
    public string Error { get; }
    public JobApplication? Application { get; }

    private ApplicationResult(bool success, string error, JobApplication? application)
    {
        Success = success;
        Error = error;
        Application = application;
    }

    public static ApplicationResult CreateSuccess(JobApplication app) => 
        new(true, null, app);
    
    public static ApplicationResult CreateError(string error) => 
        new(false, error, null);
} 