using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Storage;
using Xunit;

namespace FEwS.Forums.E2E;

public class AccountEndpointsShould(ForumApiApplicationFactory factory) : IClassFixture<ForumApiApplicationFactory>
{
    [Fact]
    public async Task SignInAfterSignOn()
    {
        using HttpClient httpClient = factory.CreateClient();

        using var accountJsonContent = JsonContent.Create(new { userName = "Test", password = "qwerty" });
        using HttpResponseMessage signOnResponse = await httpClient.PostAsync(
            "account", accountJsonContent);
        signOnResponse.IsSuccessStatusCode.Should().BeTrue();
        User? createdUser = await signOnResponse.Content.ReadFromJsonAsync<User>();

        using HttpResponseMessage signInResponse = await httpClient.PostAsync(
            "account/signin", accountJsonContent);
        signInResponse.IsSuccessStatusCode.Should().BeTrue();

        User? signedInUser = await signInResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();
        signedInUser?.UserId.Should().Be(createdUser.UserId);

        using var forumJsonContent = JsonContent.Create(new { title = "Test title" });
        HttpResponseMessage createForumResponse = await httpClient.PostAsync(
            "forums", forumJsonContent);
        createForumResponse.IsSuccessStatusCode.Should().BeTrue();
        API.Models.Forum forum = await createForumResponse.Content.ReadFromJsonAsync<API.Models.Forum>() ?? throw new InvalidOperationException();

        using var topicJsonContent = JsonContent.Create(new { title = "New topic" });
        HttpResponseMessage createTopicResponse = await httpClient.PostAsync(
            $"forums/{forum.Id}/topics", topicJsonContent);
        createTopicResponse.IsSuccessStatusCode.Should().BeTrue();

        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        Storage.Entities.DomainEvent[] domainEvents = await scope.ServiceProvider.GetRequiredService<ForumDbContext>()
            .DomainEvents.ToArrayAsync();
        domainEvents.Should().HaveCount(1);
    }
}