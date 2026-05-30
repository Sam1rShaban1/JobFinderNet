using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface INotificationService
{
    Task SendApplicationSubmittedAsync(string applicantEmail, string applicantName, string jobTitle, string companyName);
    Task SendApplicationStatusChangedAsync(string applicantEmail, string applicantName, string jobTitle, ApplicationStatus newStatus);
    Task SendJobPostedAsync(string employerEmail, string employerName, string jobTitle);
    Task SendNewApplicationToEmployerAsync(string employerEmail, string employerName, string applicantName, string jobTitle);
    Task SendMatchNotificationAsync(string email, string userName, string jobTitle, string companyName, int score, string applyLink);
    Task SendDigestAsync(string email, string userName, List<PendingDigest> matches, string frequency);
}
