using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User = FEwS.Forums.Domain.Models.User;

namespace FEwS.Forums.Domain.UseCases.SignIn;

internal class SignInUseCase(
    ISignInStorage storage,
    IPasswordHasher<User> passwordHasher,
    ISymmetricEncryptor encryptor)
    : IRequestHandler<SignInCommand, (IIdentity identity, string token)>
{
    public async Task<(IIdentity identity, string token)> Handle(
        SignInCommand command, CancellationToken cancellationToken)
    {
        var recognisedUser = await storage.FindUserAsync(command.UserName, cancellationToken);
        if (recognisedUser is null)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = nameof(command.UserName),
                    ErrorCode = ValidationErrorCode.Invalid,
                    AttemptedValue = command.UserName
                }
            ]);
        }
        var passwordVerificationResult = passwordHasher
            .VerifyHashedPassword(recognisedUser, recognisedUser.PasswordHash, command.Password);
        var passwordMatches = passwordVerificationResult 
                              == PasswordVerificationResult.Success;;
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
        return (new Authentication.User(recognisedUser.UserId, sessionId), token);
    }
}