using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface INotificationService
{
    Task SendApplicationSubmittedAsync(string applicantEmail, string jobTitle);
    Task SendApplicationStatusChangedAsync(string applicantEmail, string jobTitle, ApplicationStatus newStatus);
    Task SendJobPostedAsync(string employerEmail, string jobTitle);
}
