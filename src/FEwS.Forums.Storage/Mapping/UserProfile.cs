using AutoMapper;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Storage.Entities;
using User = FEwS.Forums.Domain.Models.User;

namespace FEwS.Forums.Storage.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Entities.User, User>()
            .ForMember(d => d.UserId, s => s.MapFrom(u => u.Id));
        CreateMap<Session, FEwS.Forums.Domain.Authentication.Session>();
    }
}