using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class SavedJobsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SavedJobsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetSavedJobs_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/savedjobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSavedJobIds_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/savedjobs/ids");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
