using FEwS.Forums.Domain.UseCases.SignOut;

namespace FEwS.Forums.Storage.Storages;

internal class SignOutStorage : ISignOutStorage
{
    public Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}