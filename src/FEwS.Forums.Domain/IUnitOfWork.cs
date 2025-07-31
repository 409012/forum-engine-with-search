namespace FEwS.Forums.Domain;

public interface IUnitOfWork
{
    Task<IUnitOfWorkScope> StartScope(CancellationToken cancellationToken);
}

public interface IUnitOfWorkScope : IAsyncDisposable
{
    TStorage GetStorage<TStorage>() where TStorage : IStorage;
    Task CommitAsync(CancellationToken cancellationToken);
}

public interface IStorage;