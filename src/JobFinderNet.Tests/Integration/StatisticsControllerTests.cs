using System.Net;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class StatisticsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StatisticsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetStatistics_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/statistics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployerDashboard_AsApplicant_ReturnsForbid()
    {
        var response = await _client.GetAsync("/api/statistics/employer");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
