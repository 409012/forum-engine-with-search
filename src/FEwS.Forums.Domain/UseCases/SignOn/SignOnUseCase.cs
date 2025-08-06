using FEwS.Forums.Domain.Authentication;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User = FEwS.Forums.Domain.Models.User;

namespace FEwS.Forums.Domain.UseCases.SignOn;

internal class SignOnUseCase(
    IPasswordHasher<User> passwordHasher,
    ISignOnStorage storage)
    : IRequestHandler<SignOnCommand, IIdentity>
{
    public async Task<IIdentity> Handle(SignOnCommand command, CancellationToken cancellationToken)
    {
        var user = new User();
        var passwordHash = passwordHasher.HashPassword(user, command.Password);
        var userId = await storage.CreateUserAsync(command.UserName, passwordHash, cancellationToken);

        return new Authentication.User(userId, Guid.Empty);
    }
}