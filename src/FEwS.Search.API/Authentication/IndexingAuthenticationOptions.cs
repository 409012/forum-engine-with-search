using Microsoft.AspNetCore.Authentication;

namespace FEwS.Search.API.Authentication;

internal class IndexingAuthenticationOptions : AuthenticationSchemeOptions
{
    internal const string SchemeName = "Indexing";
    internal const string HeaderName = "x-indexing-key";

    public string ApiKey { get; set; } = string.Empty;
}
