using BlazorBase.Server.User.Models;
using BlazorBase.Server.User.Services;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.Server.User;

public static class BlazorBaseServerUserConfiguration
{
    /// <summary>
    /// Register blazor base server user services.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseServerUser<TUser, TIdentityUser, TIdentityRole, TOptions>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null)
        where TUser : class, IBaseUser<TIdentityUser, TIdentityRole>, new()
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : struct, Enum
        where TOptions : class, IBlazorBaseServerUserOptions, new()
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection
          .AddSingleton(configureOptions)
          .AddTransient<IBlazorBaseServerUserOptions, TOptions>();

        var options = new TOptions();
        configureOptions.Invoke(options);

        if (options.LogUserSessions)
        {
            if (!typeof(IBaseUserSessionData).IsAssignableFrom(typeof(TUser)))
                throw new NotSupportedException($"The type {typeof(TUser)} must implement the interface IBaseUserSessionData, so that the user sessions can be logged into the user table");

            serviceCollection.AddScoped<CircuitHandler, BaseUserCircuitHandlerService>();
        }

        return serviceCollection;
    }
}
