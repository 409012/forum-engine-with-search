using FEwS.Forums.Domain.DomainEvents;

namespace FEwS.Forums.Domain.UseCases;

public interface IDomainEventStorage : IStorage
{
    Task AddEventAsync(ForumDomainEvent domainEvent, CancellationToken cancellationToken);
}