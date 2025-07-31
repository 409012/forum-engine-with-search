using AutoMapper;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Storage.Entities;

namespace FEwS.Forums.Storage.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, RecognisedUser>();
        CreateMap<Session, FEwS.Forums.Domain.Authentication.Session>();
    }
}