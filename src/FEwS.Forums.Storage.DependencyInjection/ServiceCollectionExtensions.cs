using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FEwS.Forums.Domain;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.UseCases;
using FEwS.Forums.Domain.UseCases.CreateComment;
using FEwS.Forums.Domain.UseCases.CreateForum;
using FEwS.Forums.Domain.UseCases.CreateTopic;
using FEwS.Forums.Domain.UseCases.GetForums;
using FEwS.Forums.Domain.UseCases.GetTopics;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Domain.UseCases.SignOn;
using FEwS.Forums.Domain.UseCases.SignOut;
using FEwS.Forums.Storage.Storages;

namespace FEwS.Forums.Storage.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForumStorage(this IServiceCollection services, string dbConnectionString)
    {
        services
            .AddScoped<IDomainEventStorage, DomainEventStorage>()
            .AddScoped<IAuthenticationStorage, AuthenticationStorage>()
            .AddScoped<ICreateForumStorage, CreateForumStorage>()
            .AddScoped<IGetForumsStorage, GetForumsStorage>()
            .AddScoped<ICreateTopicStorage, CreateTopicStorage>()
            .AddScoped<IGetTopicsStorage, GetTopicsStorage>()
            .AddScoped<ICreateCommentStorage, CreateCommentStorage>()
            .AddScoped<ISignOnStorage, SignOnStorage>()
            .AddScoped<ISignInStorage, SignInStorage>()
            .AddScoped<ISignOutStorage, SignOutStorage>()
            .AddScoped<IGuidFactory, GuidFactory>()
            .AddDbContextPool<ForumDbContext>(options => options
                .UseNpgsql(dbConnectionString));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IUnitOfWork, UnitOfWork<ForumDbContext>>();

        services.AddMemoryCache();

        services.AddAutoMapper(config => config
            .AddMaps(Assembly.GetAssembly(typeof(ForumDbContext))));

        return services;
    }
}