using FEwS.Forums.Domain.Authentication;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.SignOn;

internal class SignOnUseCase(
    IPasswordManager passwordManager,
    ISignOnStorage storage)
    : IRequestHandler<SignOnCommand, IIdentity>
{
    public async Task<IIdentity> Handle(SignOnCommand command, CancellationToken cancellationToken)
    {
        var (salt, hash) = passwordManager.GeneratePasswordParts(command.Password);
        var userId = await storage.CreateUserAsync(command.Login, salt, hash, cancellationToken);

        return new User(userId, Guid.Empty);
    }
}