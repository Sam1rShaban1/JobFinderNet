using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class ResumeControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResumeControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task ParseResume_WithText_ReturnsOk()
    {
        var request = new ParseResumeRequest
        {
            ResumeText = "John Doe, Software Engineer with 5 years experience in C# and React."
        };

        var response = await _client.PostAsJsonAsync("/api/resume/parse", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ParsedResume>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Skills);
    }

    [Fact]
    public async Task ParseResume_WithEmptyRequest_ReturnsOkWithMock()
    {
        var request = new ParseResumeRequest();

        var response = await _client.PostAsJsonAsync("/api/resume/parse", request);

        // MockAiService always returns a result; in production the LLM may reject empty input
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GenerateCoverLetter_WithValidRequest_ReturnsOk()
    {
        var request = new CoverLetterRequest
        {
            JobTitle = "Senior Software Engineer",
            CompanyName = "Tech Corp",
            JobDescription = "Looking for a senior developer with C# experience."
        };

        var response = await _client.PostAsJsonAsync("/api/resume/cover-letter", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CoverLetterResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.CoverLetter);
    }

    [Fact]
    public async Task GenerateCoverLetter_WithEmptyFields_ReturnsBadRequest()
    {
        var request = new CoverLetterRequest
        {
            JobTitle = "",
            CompanyName = ""
        };

        var response = await _client.PostAsJsonAsync("/api/resume/cover-letter", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRecommendations_FromSkills_ReturnsOk()
    {
        var skills = new List<string> { "C#", "React", "PostgreSQL" };

        var response = await _client.PostAsJsonAsync("/api/resume/recommendations/from-skills?limit=5", skills);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<MatchedJobDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
