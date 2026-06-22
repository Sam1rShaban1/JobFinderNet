using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class CompanyProfilesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CompanyProfilesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetCompanyProfile_NotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/companyprofiles/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchCompanies_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/companyprofiles?q=test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchCompanies_NoQuery_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/companyprofiles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMyCompany_NoCompany_ReturnsNoContent()
    {
        var response = await _client.GetAsync("/api/companyprofiles/my");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
