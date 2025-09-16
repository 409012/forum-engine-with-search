namespace FEwS.Forums.Domain.Authentication;

public interface IIdentity
{
    Guid UserId { get; }
    Guid SessionId { get; }
}

internal static class IdentityExtensions
{
    public static bool IsAuthenticated(this IIdentity identity)
    {
        return identity.UserId != Guid.Empty;
    }
}
