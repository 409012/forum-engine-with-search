using AutoMapper;
using FEwS.Forums.Storage.Entities;

namespace FEwS.Forums.Storage.Mapping;

internal class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, FEwS.Forums.Domain.Models.Comment>()
            .ForMember(c => c.Id, s => s.MapFrom(c => c.CommentId));
    }
}