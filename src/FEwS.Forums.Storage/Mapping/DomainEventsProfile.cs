using AutoMapper;
using ForumDomainEvent = FEwS.Forums.Storage.Models.ForumDomainEvent;
using Models_ForumDomainEvent = FEwS.Forums.Storage.Models.ForumDomainEvent;

namespace FEwS.Forums.Storage.Mapping;

public class DomainEventsProfile : Profile
{
    public DomainEventsProfile()
    {
        CreateMap<FEwS.Forums.Domain.DomainEvents.ForumDomainEvent, Models_ForumDomainEvent>();
        CreateMap<FEwS.Forums.Domain.DomainEvents.ForumDomainEvent.ForumComment, Models_ForumDomainEvent.ForumComment>();
    }
}