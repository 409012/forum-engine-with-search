using FEwS.Forums.Domain.Authentication;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.SignOut;

internal class SignOutUseCase(
    IIdentityProvider identityProvider,
    ISignOutStorage storage)
    : IRequestHandler<SignOutCommand>
{
    public Task Handle(SignOutCommand command, CancellationToken cancellationToken)
    {
        return storage.RemoveSessionAsync(identityProvider.Current.SessionId, cancellationToken);
    }
}