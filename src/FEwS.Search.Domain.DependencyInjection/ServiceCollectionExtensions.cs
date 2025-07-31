using Microsoft.Extensions.DependencyInjection;
using FEwS.Search.Domain.Models;

namespace FEwS.Search.Domain.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchDomain(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SearchEntity>());

        return services;
    }
}