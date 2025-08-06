using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.SignIn;

public interface ISignInStorage
{
    Task<User?> FindUserAsync(string userName, CancellationToken cancellationToken);

    Task<Guid> CreateSessionAsync(Guid userId, DateTimeOffset expirationMoment, CancellationToken cancellationToken);
}