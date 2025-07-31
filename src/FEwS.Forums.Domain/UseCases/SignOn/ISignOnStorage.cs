namespace FEwS.Forums.Domain.UseCases.SignOn;

public interface ISignOnStorage
{
    Task<Guid> CreateUserAsync(string login, byte[] salt, byte[] hash, CancellationToken cancellationToken);
}