using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class ProfileControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetProfile_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/userprofile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMatchedJobs_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/userprofile/matched?limit=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMatchedJobsDetailed_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/userprofile/matched/detailed?limit=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailableSkills_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/userprofile/skills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
