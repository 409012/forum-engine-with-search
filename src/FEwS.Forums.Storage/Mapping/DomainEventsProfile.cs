using AutoMapper;
using Models_ForumDomainEvent = FEwS.Forums.Storage.Models.ForumDomainEvent;

namespace FEwS.Forums.Storage.Mapping;

public class DomainEventsProfile : Profile
{
    public DomainEventsProfile()
    {
        CreateMap<Domain.DomainEvents.ForumDomainEvent, Models_ForumDomainEvent>();
        CreateMap<Domain.DomainEvents.ForumDomainEvent.ForumComment, Models_ForumDomainEvent.ForumComment>();
    }
}