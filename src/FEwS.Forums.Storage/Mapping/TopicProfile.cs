using AutoMapper;
using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Storage.Mapping;

internal class TopicProfile : Profile
{
    public TopicProfile()
    {
        CreateMap<Entities.Topic, Topic>()
            .ForMember(d => d.Id, s => s.MapFrom(t => t.TopicId));

        CreateMap<Models.TopicReadModel, TopicReadModel>()
            .ForMember(d => d.Id, s => s.MapFrom(t => t.TopicId))
            .ForMember(d => d.LastComment, s => s.MapFrom(t
                => new TopicTopicReadModelComment
                {
                    Id = t.LastCommentId,
                    CreatedAt = t.LastCommentCreatedAt
                }));
    }
}