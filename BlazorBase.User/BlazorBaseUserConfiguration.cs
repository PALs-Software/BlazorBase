using BlazorBase.User.Models;
using BlazorBase.User.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase.User;
public static class BlazorBaseUserConfiguration
{
    /// <summary>
    /// Register blazor base user handling and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseUserManagement<TUser, TIdentityUser, TIdentityRole>(this IServiceCollection serviceCollection)
        where TUser : BaseUser<TIdentityUser, TIdentityRole>, new()
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : struct, Enum
    {
        serviceCollection
            .AddTransient<BaseUserService<TUser, TIdentityUser, TIdentityRole>>();

        return serviceCollection;
    }
}
