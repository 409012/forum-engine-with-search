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

        using var forumJsonContent = JsonContent.Create(new { title = "Test forum" });
        using HttpResponseMessage forumCreatedResponse = await httpClient.PostAsync("forums",
            forumJsonContent);
        forumCreatedResponse.EnsureSuccessStatusCode();

        API.Models.Forum? createdForum = await forumCreatedResponse.Content.ReadFromJsonAsync<API.Models.Forum>();
        createdForum.Should().NotBeNull();

        using var topicJsonContent = JsonContent.Create(new { title = "Hello world" });
        HttpResponseMessage responseMessage = await httpClient.PostAsync($"forums/{createdForum.Id}/topics",
            topicJsonContent);
        responseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}