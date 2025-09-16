using FEwS.Forums.API.Authentication;
using FEwS.Forums.Domain.Authentication;

namespace FEwS.Forums.API.Middlewares;

public class AuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        IAuthTokenStorage tokenStorage,
        IAuthenticationService authenticationService,
        IIdentityProvider identityProvider)
    {
        IIdentity identity = tokenStorage.TryExtract(httpContext, out string authToken)
            ? await authenticationService.AuthenticateAsync(authToken, httpContext.RequestAborted)
            : User.Guest;
        identityProvider.Current = identity;

        await next(httpContext);
    }
}