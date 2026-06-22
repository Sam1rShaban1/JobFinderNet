using System.Net;
using System.Net.Http.Json;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Integration;

public class NotificationsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotificationsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetNotifications_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOk()
    {
        var response = await _client.PutAsJsonAsync("/api/notifications/read-all", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
