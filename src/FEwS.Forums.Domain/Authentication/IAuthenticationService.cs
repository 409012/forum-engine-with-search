namespace FEwS.Forums.Domain.Authentication;

public interface IAuthenticationService
{
    Task<IIdentity> AuthenticateAsync(string authToken, CancellationToken cancellationToken);
}