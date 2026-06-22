using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class SavedSearchesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SavedSearchesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetSavedSearches_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/savedsearches");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateSavedSearch_WithProfile_ReturnsOkOrBadRequest()
    {
        var dto = new SavedSearchDto { Name = "Test Search", EmailFrequency = "daily" };
        var response = await _client.PostAsJsonAsync("/api/savedsearches", dto);

        // Returns 200 if profile exists, 400 if not
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }
}
