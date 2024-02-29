using BlazorBase.Mailing.Services;
using BlazorBase.User.Controller;
using BlazorBase.User.Enums;
using BlazorBase.User.Models;
using BlazorBase.User.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
    public static IServiceCollection AddBlazorBaseUserManagement<TUserService, TUser, TIdentityUser, TIdentityRole, TOptions>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null)
        where TUserService : class, IBaseUserService, IBaseUserService<TUser, TIdentityUser, TIdentityRole>
        where TUser : class, IBaseUser<TIdentityUser, TIdentityRole>, new()
        where TIdentityUser : IdentityUser, new()
        where TIdentityRole : struct, Enum
        where TOptions : class, IBlazorBaseUserOptions, new()
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection
            .AddSingleton(configureOptions)
            .AddTransient<IBlazorBaseUserOptions, TOptions>()
            .AddSingleton<IBaseUser, TUser>()
            .AddTransient<IBaseUserService, TUserService>()
            .AddTransient<TUserService>()

        .AddControllers().AddApplicationPart(typeof(UserLoginController).Assembly).AddControllersAsServices();

        if (OperatingSystem.IsWindows())
            serviceCollection.AddTransient<BaseMailService<UserMailTemplate>>();

        var options = new TOptions();
        configureOptions.Invoke(options);

        serviceCollection.ConfigureApplicationCookie(acOptions =>
        {
            acOptions.LoginPath = options.LoginPath;
            acOptions.AccessDeniedPath = options.IdentityAccessDeniedPath;
        });
      
        return serviceCollection;
    }

    public static void RemoveDefaultIdentityControllerEndpoints(this WebApplication app, string defaultRedirectPath = "~/")
    {
        /*
        app.MapGet("/Identity/Account/Login", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Login", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/LoginWith2fa", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/LoginWith2fa", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/LoginWithRecoveryCode", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/LoginWithRecoveryCode", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/Lockout", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Lockout", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/ExternalLogin", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ExternalLogin", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/LogOut", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/LogOut", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/ResendEmailConfirmation", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ResendEmailConfirmation", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/ConfirmEmail", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ConfirmEmail", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/ConfirmEmailChange", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ConfirmEmailChange", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/RegisterConfirmationModel", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/RegisterConfirmationModel", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
                
        app.MapGet("/Identity/Account/ForgotPassword", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ForgotPassword", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/ResetPassword", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ResetPassword", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/ResetPasswordConfirmation", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/ResetPasswordConfirmation", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        
        app.MapGet("/Identity/Account/Manage", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Manage", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/Manage/Email", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Manage/Email", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/Manage/ChangePassword", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Manage/ChangePassword", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/Manage/TwoFactorAuthentication", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Manage/TwoFactorAuthentication", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));

        app.MapGet("/Identity/Account/Manage/PersonalData", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        app.MapPost("/Identity/Account/Manage/PersonalData", context => Task.Factory.StartNew(() => context.Response.Redirect(defaultRedirectPath, true, true)));
        */
    }
}
