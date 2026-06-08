using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Services;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly ApplicationDbContext _context;
    private readonly IMatchingService _matchingService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NvidiaOptions _nvidiaOptions;

    private const string NVIDIA_API_URL = "https://integrate.api.nvidia.com/v1/chat/completions";
    private const string MODEL = "moonshotai/kimi-k2.6";

    public AiService(
        ApplicationDbContext context,
        IMatchingService matchingService,
        IHttpClientFactory httpClientFactory,
        IOptions<NvidiaOptions> nvidiaOptions)
    {
        _context = context;
        _matchingService = matchingService;
        _httpClientFactory = httpClientFactory;
        _nvidiaOptions = nvidiaOptions.Value;
    }

    public async Task<ParsedResume> ParseResumeAsync(ParseResumeRequest request)
    {
        if (!string.IsNullOrEmpty(request.ResumeText))
        {
            return await ParseTextWithLlm(request.ResumeText);
        }

        if (!string.IsNullOrEmpty(request.ImageBase64))
        {
            if (request.IsPdf)
            {
                var text = ExtractTextFromPdfBytes(Convert.FromBase64String(request.ImageBase64));
                if (!string.IsNullOrWhiteSpace(text))
                    return await ParseTextWithLlm(text);

                throw new ArgumentException("Could not extract text from PDF. Try pasting the resume text instead.");
            }

            var imageText = await ExtractTextFromImage(request.ImageBase64, request.ImageMediaType ?? "image/png");
            return await ParseTextWithLlm(imageText);
        }

        throw new ArgumentException("Either ResumeText, ImageBase64, or a file must be provided");
    }

    public async Task<List<MatchedJobDto>> GetRecommendationsAsync(string userId, ParsedResume resume, int limit = 10)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            _context.UserProfiles.Add(profile);
        }

        var newSkills = resume.Skills
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingSkills = profile.Skills
            .Select(s => s.ToLowerInvariant())
            .ToHashSet();

        foreach (var skill in newSkills)
        {
            if (!existingSkills.Contains(skill.ToLowerInvariant()))
            {
                profile.Skills.Add(skill);
            }
        }

        if (!string.IsNullOrEmpty(resume.SeniorityLevel) && string.IsNullOrEmpty(profile.SeniorityLevel))
        {
            profile.SeniorityLevel = resume.SeniorityLevel;
        }

        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var matches = await _matchingService.GetTopMatchesDetailed(profile, limit);
        return matches;
    }

    private async Task<string> ExtractTextFromImage(string imageBase64, string mediaType)
    {
        var prompt = @"Extract ALL text from this resume image. Return the complete text content exactly as it appears, preserving structure and formatting. Return ONLY the raw text, no explanation.";

        var content = new List<object>
        {
            new { type = "text", text = prompt },
            new
            {
                type = "image_url",
                image_url = new { url = $"data:{mediaType};base64,{imageBase64}" }
            }
        };

        return await CallNvidiaLlm(content);
    }

    private async Task<ParsedResume> ParseTextWithLlm(string resumeText)
    {
        var prompt = $@"Parse this resume and extract structured data. Return ONLY valid JSON with this exact schema:
{{
  ""skills"": [""skill1"", ""skill2"", ...],
  ""seniorityLevel"": ""Junior"" | ""Mid-Level"" | ""Senior"" | ""Lead"" | ""Manager"",
  ""experienceYears"": 5,
  ""education"": [
    {{ ""degree"": ""B.S. Computer Science"", ""institution"": ""MIT"", ""year"": ""2020"" }}
  ],
  ""summary"": ""Brief professional summary"",
  ""jobTitles"": [""Previous job titles held""]
}}

Rules:
- skills: ALL technical skills, programming languages, frameworks, tools, databases, cloud platforms mentioned
- seniorityLevel: infer from years of experience and job titles
- experienceYears: total years of professional experience
- jobTitles: all job titles mentioned in the resume

Resume text:
{resumeText}";

        var content = new List<object>
        {
            new { type = "text", text = prompt }
        };

        var response = await CallNvidiaLlm(content);

        try
        {
            var jsonStr = ExtractJson(response);
            var result = JsonSerializer.Deserialize<ParsedResume>(jsonStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result ?? new ParsedResume { Summary = response };
        }
        catch
        {
            return new ParsedResume
            {
                Summary = response.Length > 500 ? response[..500] : response
            };
        }
    }

    private async Task<string> CallNvidiaLlm(List<object> messageContent)
    {
        var apiKey = _nvidiaOptions.ApiKey
            ?? throw new InvalidOperationException("NVIDIA API key not configured");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var payload = new
        {
            model = MODEL,
            messages = new[]
            {
                new { role = "user", content = messageContent }
            },
            max_tokens = 4096,
            temperature = 0.3,
            top_p = 1.0,
            stream = false
        };

        var json = JsonSerializer.Serialize(payload);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(NVIDIA_API_URL, httpContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"NVIDIA API error ({response.StatusCode}): {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var result = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return result ?? string.Empty;
    }

    public async Task<CoverLetterResponse> GenerateCoverLetterAsync(string userId, CoverLetterRequest request)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        var skills = profile?.Skills.Any() == true
            ? string.Join(", ", profile.Skills)
            : "Not specified";

        var seniority = profile?.SeniorityLevel ?? "Not specified";

        var salaryNote = "";
        if (profile?.DesiredSalaryMin != null && profile?.DesiredSalaryMax != null)
            salaryNote = $"Expected salary range: ${profile.DesiredSalaryMin:N0} - ${profile.DesiredSalaryMax:N0}";

        var remoteNote = profile?.IsOpenToRemote == true ? "Open to remote work." : "";

        var tone = string.IsNullOrEmpty(request.Tone) ? "professional" : request.Tone.ToLowerInvariant();

        var prompt = $@"Write a professional cover letter for the following job application.

Candidate Profile:
- Skills: {skills}
- Seniority Level: {seniority}
{salaryNote}
{remoteNote}

Job Details:
- Position: {request.JobTitle}
- Company: {request.CompanyName}
{(string.IsNullOrEmpty(request.JobDescription) ? "" : $"- Job Description: {request.JobDescription}")}
{(string.IsNullOrEmpty(request.HiringManager) ? "" : $"- Hiring Manager: {request.HiringManager}")}

Tone: {tone}

Requirements:
1. Start with a strong opening paragraph that mentions the specific role and company
2. Highlight 3-5 relevant skills from the candidate's profile that match the role
3. Include a concrete example or achievement (infer a reasonable one based on the skills)
4. Show knowledge of the company's mission or industry
5. Close with a confident call to action

Format as a proper business letter with:
- Date
- Greeting
- 3-4 body paragraphs
- Professional closing

Keep it under 400 words. Do not use placeholder text like [Company Name] — write it out.";

        var content = new List<object>
        {
            new { type = "text", text = prompt }
        };

        var coverLetter = await CallNvidiaLlm(content);

        string tips = "";
        try
        {
            var tipsPrompt = $@"Based on this cover letter for a {request.JobTitle} position at {request.CompanyName}, provide 3 brief actionable tips to make it even stronger. Format as a numbered list, one tip per line. Keep each tip under 15 words.";

            var tipsContent = new List<object>
            {
                new { type = "text", text = tipsPrompt }
            };

            tips = await CallNvidiaLlm(tipsContent);
        }
        catch
        {
            // Tips are optional — don't fail the whole request
        }

        return new CoverLetterResponse
        {
            CoverLetter = coverLetter.Trim(),
            Tips = tips.Trim()
        };
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return text[start..(end + 1)];
        }
        return text;
    }

    private static string ExtractTextFromPdfBytes(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfDocument.Open(stream);
        var text = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }
        return text.ToString();
    }
}
