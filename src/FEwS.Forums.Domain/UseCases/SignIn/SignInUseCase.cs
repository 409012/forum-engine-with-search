using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Options;

namespace FEwS.Forums.Domain.UseCases.SignIn;

internal class SignInUseCase(
    ISignInStorage storage,
    IPasswordManager passwordManager,
    ISymmetricEncryptor encryptor)
    : IRequestHandler<SignInCommand, (IIdentity identity, string token)>
{
    public async Task<(IIdentity identity, string token)> Handle(
        SignInCommand command, CancellationToken cancellationToken)
    {
        var recognisedUser = await storage.FindUserAsync(command.Login, cancellationToken);
        if (recognisedUser is null)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = nameof(command.Login),
                    ErrorCode = ValidationErrorCode.Invalid,
                    AttemptedValue = command.Login
                }
            ]);
        }

        var passwordMatches = passwordManager.ComparePasswords(
            command.Password, recognisedUser.Salt, recognisedUser.PasswordHash);
        if (!passwordMatches)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = nameof(command.Password),
                    ErrorCode = ValidationErrorCode.Invalid,
                    AttemptedValue = command.Password
                }
            ]);
        }

        var sessionId = await storage.CreateSessionAsync(
            recognisedUser.UserId, DateTimeOffset.UtcNow + TimeSpan.FromHours(1), cancellationToken);
        var token = await encryptor.EncryptAsync(sessionId.ToString(), cancellationToken);
        return (new User(recognisedUser.UserId, sessionId), token);
    }
}