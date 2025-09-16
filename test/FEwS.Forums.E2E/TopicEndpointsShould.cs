using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FEwS.Forums.E2E;

public class TopicEndpointsShould(ForumApiApplicationFactory factory)
    : IClassFixture<ForumApiApplicationFactory>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    // [Fact]
    public async Task ReturnForbiddenWhenNotAuthenticated()
    {
        using HttpClient httpClient = factory.CreateClient();

        using HttpResponseMessage forumCreatedResponse = await httpClient.PostAsync("forums",
            JsonContent.Create(new { title = "Test forum" }));
        forumCreatedResponse.EnsureSuccessStatusCode();

        API.Models.Forum? createdForum = await forumCreatedResponse.Content.ReadFromJsonAsync<API.Models.Forum>();
        createdForum.Should().NotBeNull();

        HttpResponseMessage responseMessage = await httpClient.PostAsync($"forums/{createdForum.Id}/topics",
            JsonContent.Create(new { title = "Hello world" }));
        responseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}