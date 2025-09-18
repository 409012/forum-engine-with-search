using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace FEwS.Forums.E2E;

public class ForumEndpointsShould(ForumApiApplicationFactory factory) : IClassFixture<ForumApiApplicationFactory>
{
    // [Fact]
    public async Task CreateNewForum()
    {
        const string forumTitle = "0069517D-CA29-453B-BB4C-AC22F51E690E";

        using HttpClient httpClient = factory.CreateClient();

        using HttpResponseMessage getInitialForumsResponse = await httpClient.GetAsync("forums");
        getInitialForumsResponse.IsSuccessStatusCode.Should().BeTrue();
        API.Models.Forum[]? initialForums = await getInitialForumsResponse.Content.ReadFromJsonAsync<API.Models.Forum[]>();
        initialForums
            .Should().NotBeNull().And
            .Subject.As<API.Models.Forum[]>().Should().NotContain(f => f.Title.Equals(forumTitle));

        using var forumJsonContent = JsonContent.Create(new { title = forumTitle });
        using HttpResponseMessage response = await httpClient.PostAsync("forums",
            forumJsonContent);

        response.IsSuccessStatusCode.Should().BeTrue();
        API.Models.Forum? forum = await response.Content.ReadFromJsonAsync<API.Models.Forum>();
        forum
            .Should().NotBeNull().And
            .Subject.As<API.Models.Forum>().Title.Should().Be(forumTitle);

        using HttpResponseMessage getForumsResponse = await httpClient.GetAsync("forums");
        API.Models.Forum[]? forums = await getForumsResponse.Content.ReadFromJsonAsync<API.Models.Forum[]>();
        forums
            .Should().NotBeNull().And
            .Subject.As<API.Models.Forum[]>().Should().Contain(f => f.Title.Equals(forumTitle));
    }
}