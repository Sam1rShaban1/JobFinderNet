using JobFinderNet.Models;

namespace JobFinderNet.Models;

public class ApplicationResult
{
    public bool Succeeded { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public Application? Application { get; private set; }

    public static ApplicationResult CreateSuccess(Application application)
    {
        return new ApplicationResult 
        { 
            Succeeded = true,
            Application = application,
            Message = "Application submitted successfully"
        };
    }

    public static ApplicationResult CreateError(string message)
    {
        return new ApplicationResult 
        { 
            Succeeded = false,
            Message = message
        };
    }
} 