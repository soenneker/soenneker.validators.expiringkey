using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Validators.ExpiringKey.Abstract;

namespace Soenneker.Validators.ExpiringKey.Registrars;

/// <summary>
/// A validation module that checks for keys, stores them, expires them after an amount of time
/// </summary>
public static class ExpiringKeyValidatorRegistrar
{
    /// <summary>
    /// Adds <see cref="IExpiringKeyValidator"/> as a singleton service. <para/>
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddExpiringKeyValidatorAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IExpiringKeyValidator, ExpiringKeyValidator>();
        return services;
    }

    /// <summary>
    /// Adds <see cref="IExpiringKeyValidator"/> as a scoped service. <para/>
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddExpiringKeyValidatorAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IExpiringKeyValidator, ExpiringKeyValidator>();
        return services;
    }
}
