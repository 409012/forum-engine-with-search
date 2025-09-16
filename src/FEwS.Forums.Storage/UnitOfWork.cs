using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using FEwS.Forums.Domain;

namespace FEwS.Forums.Storage;

internal class UnitOfWork<TContext>(IServiceProvider serviceProvider) : IUnitOfWork 
    where TContext : DbContext
{
    public async Task<IUnitOfWorkScope> StartScope(CancellationToken cancellationToken)
    {
        AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        TContext dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWorkScope(scope, transaction);
    }
}

internal class UnitOfWorkScope(
    IServiceScope scope,
    IDbContextTransaction transaction) : IUnitOfWorkScope
{
    public TStorage GetStorage<TStorage>() where TStorage : IStorage
    {
        return scope.ServiceProvider.GetRequiredService<TStorage>();
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
        if (scope is IAsyncDisposable scopeAsyncDisposable)
            await scopeAsyncDisposable.DisposeAsync();
        else
            scope.Dispose();
    }
}