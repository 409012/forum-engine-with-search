namespace FEwS.Forums.Domain.UseCases.SignOn;

public interface ISignOnStorage
{
    Task<Guid> CreateUserAsync(string login, string passwordHash, CancellationToken cancellationToken);
}