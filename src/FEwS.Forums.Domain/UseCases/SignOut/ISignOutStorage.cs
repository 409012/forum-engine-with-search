namespace FEwS.Forums.Domain.UseCases.SignOut;

public interface ISignOutStorage
{
    Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken);
}