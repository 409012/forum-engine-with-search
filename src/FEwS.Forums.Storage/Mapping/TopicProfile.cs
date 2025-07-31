using AutoMapper;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Storage.Models;

namespace FEwS.Forums.Storage.Mapping;

internal class TopicProfile : Profile
{
    public TopicProfile()
    {
        CreateMap<Entities.Topic, Topic>()
            .ForMember(d => d.Id, s => s.MapFrom(t => t.TopicId));

        CreateMap<TopicListItemReadModel, Topic>()
            .ForMember(d => d.Id, s => s.MapFrom(t => t.TopicId));
    }
}