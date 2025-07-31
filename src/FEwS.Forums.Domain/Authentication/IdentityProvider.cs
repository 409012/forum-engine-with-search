namespace FEwS.Forums.Domain.Authentication;

internal class IdentityProvider : IIdentityProvider
{
    public required IIdentity Current { get; set; }
}