using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class JobsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public JobsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetJobs_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/jobs?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithQuery_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/jobs/search?query=engineer");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateJob_AsApplicant_ReturnsForbid()
    {
        var dto = new CreateJobDto
        {
            Title = "Test Job",
            Description = "Test description for the job position that is long enough",
            CompanyName = "Test Corp",
            Location = "Remote",
            JobType = "Full-time",
            Salary = "$80,000/year",
            ExperienceRequired = "1-3 years"
        };

        var response = await _client.PostAsJsonAsync("/api/jobs", dto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployerJobs_AsApplicant_ReturnsForbid()
    {
        var response = await _client.GetAsync("/api/jobs/employer");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PopulateTechnologies_AsApplicant_ReturnsForbid()
    {
        var response = await _client.PostAsJsonAsync("/api/jobs/populate-techs", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SyncJobs_AsApplicant_ReturnsForbid()
    {
        var response = await _client.PostAsJsonAsync("/api/jobs/sync", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
