using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization;
using FEwS.Forums.Domain.Authorization.AccessManagement;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.Monitoring;

namespace FEwS.Forums.Domain.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForumDomain(this IServiceCollection services)
    {
        services
            .AddMediatR(cfg => cfg
                .AddOpenBehavior(typeof(MonitoringPipelineBehavior<,>))
                .AddOpenBehavior(typeof(ValidationPipelineBehavior<,>))
                .RegisterServicesFromAssemblyContaining<Forum>());
        
        services
            .AddScoped<IIntentionResolver, ForumIntentionResolver>()
            .AddScoped<IIntentionResolver, TopicIntentionResolver>();

        services
            .AddScoped<IIntentionManager, IntentionManager>()
            .AddScoped<IIdentityProvider, IdentityProvider>()
            .AddScoped<IPasswordManager, PasswordManager>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<ISymmetricDecryptor, AesSymmetricEncryptorDecryptor>()
            .AddScoped<ISymmetricEncryptor, AesSymmetricEncryptorDecryptor>();

        services.AddValidatorsFromAssemblyContaining<Forum>(includeInternalTypes: true);

        services.AddSingleton<DomainMetrics>();

        return services;
    }
}