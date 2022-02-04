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
    /// Register also given UserService
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseUserManagement<TUserService, TUser, TIdentityUser, TIdentityRole>(this IServiceCollection serviceCollection)
        where TUserService : class, IBaseUserService<TUser, TIdentityUser, TIdentityRole>
        where TUser : class, IBaseUser<TIdentityUser, TIdentityRole>, new()
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : struct, Enum
    {
        serviceCollection
            .AddTransient<TUserService>();

        return serviceCollection;
    }
}
