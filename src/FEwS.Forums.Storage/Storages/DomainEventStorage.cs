using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using FEwS.Forums.Domain.UseCases;
using FEwS.Forums.Storage.Entities;
using ForumDomainEvent = FEwS.Forums.Domain.DomainEvents.ForumDomainEvent;

namespace FEwS.Forums.Storage.Storages;

internal class DomainEventStorage(
    ForumDbContext dbContext,
    IGuidFactory guidFactory,
    TimeProvider timeProvider,
    IMapper mapper) : IDomainEventStorage
{
    public async Task AddEventAsync(ForumDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var storageDomainEvent = mapper.Map<Models.ForumDomainEvent>(domainEvent);

        await dbContext.DomainEvents.AddAsync(new DomainEvent
        {
            DomainEventId = guidFactory.Create(),
            EmittedAt = timeProvider.GetUtcNow(),
            ContentBlob = JsonSerializer.SerializeToUtf8Bytes(storageDomainEvent),
            ActivityId = Activity.Current?.Id
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}