using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
