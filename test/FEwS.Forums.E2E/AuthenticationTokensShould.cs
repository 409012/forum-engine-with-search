using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using FEwS.Forums.Domain.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace FEwS.Forums.E2E;

public class AuthenticationTokensShould(ForumApiApplicationFactory factory) : IClassFixture<ForumApiApplicationFactory>
{
    [Theory]
    [InlineData("!")]
    [InlineData("AA==")]
    [InlineData("not-a-valid-token")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA==")]
    public async Task RejectMalformedCookieWithoutServerError(string token)
    {
        using HttpClient client = CreateClientWithToken(token);

        using HttpResponseMessage publicResponse = await client.GetAsync("forums");
        using HttpResponseMessage protectedResponse = await client.PostAsJsonAsync("forums", new { title = "Invalid token" });

        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectTamperedTokenWhileAcceptingOriginal()
    {
        using HttpClient client = factory.CreateClient();
        var credentials = new { userName = Guid.NewGuid().ToString("N")[..12], password = "password" };
        using HttpResponseMessage registration = await client.PostAsJsonAsync("account", credentials);
        registration.StatusCode.Should().Be(HttpStatusCode.OK);
        using HttpResponseMessage signIn = await client.PostAsJsonAsync("account/signin", credentials);
        signIn.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookie = SetCookieHeaderValue.Parse(signIn.Headers.GetValues("Set-Cookie").Single());
        cookie.HttpOnly.Should().BeTrue();
        cookie.Secure.Should().BeTrue();
        cookie.SameSite.Should().Be(SameSiteMode.Strict);
        cookie.Path.ToString().Should().Be("/");
        string token = cookie.Value.ToString();
        byte[] bytes = WebEncoders.Base64UrlDecode(token);
        bytes[^1] ^= 1;
        using HttpClient tamperedClient = CreateClientWithToken(WebEncoders.Base64UrlEncode(bytes));

        using HttpResponseMessage tamperedResponse = await tamperedClient.PostAsJsonAsync("forums", new { title = "Tampered" });
        using HttpResponseMessage originalResponse = await client.PostAsJsonAsync("forums", new { title = "Original" });

        tamperedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        originalResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using HttpResponseMessage signOut = await client.PostAsync("account/signout", null);
        var removedCookie = SetCookieHeaderValue.Parse(signOut.Headers.GetValues("Set-Cookie").Single());
        removedCookie.HttpOnly.Should().BeTrue();
        removedCookie.Secure.Should().BeTrue();
        removedCookie.SameSite.Should().Be(SameSiteMode.Strict);
        removedCookie.Path.Should().Be(cookie.Path);
    }

    [Fact]
    public async Task RejectTokenProtectedForAnotherPurposeOrKey()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IDataProtectionProvider provider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
        ISymmetricDecryptor decryptor = scope.ServiceProvider.GetRequiredService<ISymmetricDecryptor>();
        ISymmetricEncryptor encryptor = scope.ServiceProvider.GetRequiredService<ISymmetricEncryptor>();
        string sessionId = Guid.NewGuid().ToString();
        string token = await encryptor.EncryptAsync(sessionId, CancellationToken.None);
        (await decryptor.DecryptAsync(token, CancellationToken.None)).Should().Be(sessionId);
        string otherPurposeToken = provider.CreateProtector("AnotherPurpose").Protect(sessionId);
        string otherKeyToken = new EphemeralDataProtectionProvider()
            .CreateProtector("FEwS.Forums.Authentication.Session.v1").Protect(sessionId);

        foreach (string invalidToken in new[] { otherPurposeToken, otherKeyToken })
        {
            await decryptor.Invoking(service => service.DecryptAsync(invalidToken, CancellationToken.None))
                .Should().ThrowAsync<CryptographicException>();
        }
    }

    private HttpClient CreateClientWithToken(string token)
    {
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false
        });
        client.DefaultRequestHeaders.Add("Cookie", $"FEwS-Auth-Token={token}");
        return client;
    }
}
