using AutoMapper;
using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.API.Mapping;

internal class ApiProfile : Profile
{
    public ApiProfile()
    {
        CreateMap<Forum, Models.Forum>();
        CreateMap<Topic, Models.Topic>();
        CreateMap<Comment, Models.Comment>()
            .ForMember(d => d.AuthorLogin, s 
                => s.MapFrom(c => c.AuthorUserName));
        CreateMap<TopicTopicReadModelComment, Models.TopicTopicReadModelComment>();
        CreateMap<TopicReadModel, Models.TopicReadModel>();
        CreateMap<TopicsPagedResult, Models.TopicsPagedResult>();
    }
}