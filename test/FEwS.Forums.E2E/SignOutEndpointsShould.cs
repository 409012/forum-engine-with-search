using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace FEwS.Forums.E2E;

public class SignOutEndpointsShould(ForumApiApplicationFactory factory) : IClassFixture<ForumApiApplicationFactory>
{
    [Fact]
    public async Task RevokeCurrentSessionAndClearCookieWithoutRevokingOtherSessions()
    {
        using HttpClient client = factory.CreateClient();
        using HttpClient otherSessionClient = factory.CreateClient();
        using HttpClient replayClient = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var credentials = new { userName = Guid.NewGuid().ToString("N")[..12], password = "password" };
        using HttpResponseMessage registration = await client.PostAsJsonAsync("account", credentials);
        registration.StatusCode.Should().Be(HttpStatusCode.OK);
        using HttpResponseMessage signIn = await client.PostAsJsonAsync("account/signin", credentials);
        signIn.StatusCode.Should().Be(HttpStatusCode.OK);
        using HttpResponseMessage otherSignIn = await otherSessionClient.PostAsJsonAsync("account/signin", credentials);
        otherSignIn.StatusCode.Should().Be(HttpStatusCode.OK);
        string originalCookie = signIn.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("FEwS-Auth-Token=", StringComparison.Ordinal)).Split(';')[0];
        replayClient.DefaultRequestHeaders.Add("Cookie", originalCookie);
        using HttpResponseMessage beforeSignOut = await replayClient.PostAsJsonAsync("forums", new { title = "Before logout" });
        beforeSignOut.StatusCode.Should().Be(HttpStatusCode.Created);

        using HttpResponseMessage signOut = await client.PostAsync("account/signout", null);

        signOut.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var removedCookie = SetCookieHeaderValue.Parse(signOut.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("FEwS-Auth-Token=", StringComparison.Ordinal)));
        removedCookie.Value.ToString().Should().BeEmpty();
        removedCookie.Path.ToString().Should().Be("/");
        removedCookie.Expires.Should().NotBeNull();
        removedCookie.Expires.Value.Should().BeBefore(DateTimeOffset.UtcNow);

        using HttpResponseMessage replay = await replayClient.PostAsJsonAsync("forums", new { title = "Replayed token" });
        replay.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using HttpResponseMessage afterSignOut = await client.PostAsJsonAsync("forums", new { title = "After logout" });
        afterSignOut.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using HttpResponseMessage otherSession = await otherSessionClient.PostAsJsonAsync("forums", new { title = "Other session" });
        otherSession.StatusCode.Should().Be(HttpStatusCode.Created);

        using HttpResponseMessage repeated = await client.PostAsync("account/signout", null);
        repeated.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AllowSignOutWithoutAuthentication()
    {
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync("account/signout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.GetValues("Set-Cookie").Should()
            .Contain(value => value.StartsWith("FEwS-Auth-Token=;", StringComparison.Ordinal));
    }
}
