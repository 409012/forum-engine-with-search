namespace FEwS.Forums.Domain.UseCases.SignIn;

public interface ISignInStorage
{
    Task<RecognisedUser?> FindUserAsync(string login, CancellationToken cancellationToken);

    Task<Guid> CreateSessionAsync(Guid userId, DateTimeOffset expirationMoment, CancellationToken cancellationToken);
}