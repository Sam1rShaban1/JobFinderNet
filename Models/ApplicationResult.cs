using JobFinderNet.Models;

namespace JobFinderNet.Models;

public class ApplicationResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }
    public Application? Application { get; private set; }

    private ApplicationResult(bool success, string message, Application? application = null)
    {
        Success = success;
        Message = message;
        Application = application;
    }

    public static ApplicationResult CreateSuccess(Application application) 
        => new(true, "Application submitted successfully", application);

    public static ApplicationResult CreateError(string message) 
        => new(false, message);
} 