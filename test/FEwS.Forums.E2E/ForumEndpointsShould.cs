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

        using var httpClient = factory.CreateClient();

        using var getInitialForumsResponse = await httpClient.GetAsync("forums");
        getInitialForumsResponse.IsSuccessStatusCode.Should().BeTrue();
        var initialForums = await getInitialForumsResponse.Content.ReadFromJsonAsync<FEwS.Forums.API.Models.Forum[]>();
        initialForums
            .Should().NotBeNull().And
            .Subject.As<FEwS.Forums.API.Models.Forum[]>().Should().NotContain(f => f.Title.Equals(forumTitle));

        using var response = await httpClient.PostAsync("forums",
            JsonContent.Create(new { title = forumTitle }));

        response.IsSuccessStatusCode.Should().BeTrue();
        var forum = await response.Content.ReadFromJsonAsync<FEwS.Forums.API.Models.Forum>();
        forum
            .Should().NotBeNull().And
            .Subject.As<FEwS.Forums.API.Models.Forum>().Title.Should().Be(forumTitle);

        using var getForumsResponse = await httpClient.GetAsync("forums");
        var forums = await getForumsResponse.Content.ReadFromJsonAsync<FEwS.Forums.API.Models.Forum[]>();
        forums
            .Should().NotBeNull().And
            .Subject.As<FEwS.Forums.API.Models.Forum[]>().Should().Contain(f => f.Title.Equals(forumTitle));
    }
}