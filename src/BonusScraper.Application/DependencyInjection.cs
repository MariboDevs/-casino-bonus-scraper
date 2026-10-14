using BonusScraper.Application.Scraping;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BonusScraper.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Application services. Mediator itself is registered by the host, because
    /// its source generator emits the registration into the assembly that calls it.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ScrapeCasinosCommandValidator>(ServiceLifetime.Singleton);
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
