namespace FEwS.Forums.Domain.UseCases.SignOn;

public interface ISignOnStorage
{
    Task<Guid> CreateUserAsync(string userName, string passwordHash, CancellationToken cancellationToken);
}