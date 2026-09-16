using FEwS.Forums.Domain.Authentication;
using MediatR;
using FEwS.Forums.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
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
        string passwordHash = passwordHasher.HashPassword(user, command.Password);
        CreateUserResult result = await storage.CreateUserAsync(command.UserName, passwordHash, cancellationToken);

        return result switch
        {
            CreateUserResult.Created created => new Authentication.User(created.UserId, Guid.Empty),
            CreateUserResult.DuplicateUserName => throw new ValidationException([
                new ValidationFailure(nameof(command.UserName), "User name is already taken")
                {
                    ErrorCode = ValidationErrorCode.AlreadyExists
                }
            ]),
            _ => throw new InvalidOperationException()
        };
    }
}
