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

        using HttpResponseMessage signOnResponse = await httpClient.PostAsync(
            "account", JsonContent.Create(new { userName = "Test", password = "qwerty" }));
        signOnResponse.IsSuccessStatusCode.Should().BeTrue();
        User? createdUser = await signOnResponse.Content.ReadFromJsonAsync<User>();

        using HttpResponseMessage signInResponse = await httpClient.PostAsync(
            "account/signin", JsonContent.Create(new { userName = "Test", password = "qwerty" }));
        signInResponse.IsSuccessStatusCode.Should().BeTrue();

        User? signedInUser = await signInResponse.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();
        signedInUser?.UserId.Should().Be(createdUser.UserId);

        HttpResponseMessage createForumResponse = await httpClient.PostAsync(
            "forums", JsonContent.Create(new { title = "Test title" }));
        createForumResponse.IsSuccessStatusCode.Should().BeTrue();
        API.Models.Forum forum = await createForumResponse.Content.ReadFromJsonAsync<API.Models.Forum>() ?? throw new InvalidOperationException();

        HttpResponseMessage createTopicResponse = await httpClient.PostAsync(
            $"forums/{forum.Id}/topics", JsonContent.Create(new { title = "New topic" }));
        createTopicResponse.IsSuccessStatusCode.Should().BeTrue();

        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        Storage.Entities.DomainEvent[] domainEvents = await scope.ServiceProvider.GetRequiredService<ForumDbContext>()
            .DomainEvents.ToArrayAsync();
        domainEvents.Should().HaveCount(1);
    }
}