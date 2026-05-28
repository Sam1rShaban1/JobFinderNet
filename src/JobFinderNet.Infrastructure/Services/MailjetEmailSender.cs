using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class MailjetOptions
{
    public const string SectionName = "Mailjet";
    public string ApiKey { get; set; } = "";
    public string? ApiSecret { get; set; }
    public string FromEmail { get; set; } = "noreply@jobfinder.net";
    public string FromName { get; set; } = "JobFinder";
}

public class MailjetEmailSender : IEmailService
{
    private readonly MailjetOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MailjetEmailSender> _logger;

    public MailjetEmailSender(IOptions<MailjetOptions> options, HttpClient httpClient, ILogger<MailjetEmailSender> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var payload = new MailjetMessagePayload
        {
            Messages =
            [
                new MailjetMessage
                {
                    From = new MailjetAddress { Email = _options.FromEmail, Name = _options.FromName },
                    To = [new MailjetAddress { Email = message.To }],
                    Subject = message.Subject,
                    HtmlPart = message.IsHtml ? message.Body : null,
                    TextPart = !message.IsHtml ? message.Body : null,
                }
            ]
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send")
        {
            Content = JsonContent.Create(payload)
        };

        if (string.IsNullOrEmpty(_options.ApiSecret))
        {
            request.Headers.Authorization = new("Bearer", _options.ApiKey);
        }
        else
        {
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{_options.ApiKey}:{_options.ApiSecret}"));
            request.Headers.Authorization = new("Basic", credentials);
        }

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Mailjet send failed ({StatusCode}): {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Mailjet API returned {response.StatusCode}");
        }
    }
}

public class MailjetMessagePayload
{
    [JsonPropertyName("Messages")]
    public required List<MailjetMessage> Messages { get; init; }
}

public class MailjetMessage
{
    [JsonPropertyName("From")]
    public required MailjetAddress From { get; init; }

    [JsonPropertyName("To")]
    public required List<MailjetAddress> To { get; init; }

    [JsonPropertyName("Subject")]
    public required string Subject { get; init; }

    [JsonPropertyName("HTMLPart")]
    public string? HtmlPart { get; init; }

    [JsonPropertyName("TextPart")]
    public string? TextPart { get; init; }
}

public class MailjetAddress
{
    [JsonPropertyName("Email")]
    public required string Email { get; init; }

    [JsonPropertyName("Name")]
    public string? Name { get; init; }
}
