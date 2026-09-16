using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace FEwS.Search.API.Authentication;

internal class IndexingAuthenticationHandler(
    IOptionsMonitor<IndexingAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<IndexingAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(IndexingAuthenticationOptions.HeaderName, out StringValues values))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.IsHttps || values.Count != 1 || values[0] is not { Length: 64 } key
            || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(Options.ApiKey)))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid indexing credentials"));
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "forum-consumer")], Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
