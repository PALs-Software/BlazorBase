using BlazorBase.CRUD.Services;
using BlazorBase.User.Extensions;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.User.Services;

public class BaseUserService<TUser, TIdentityUser, TIdentityRole> : IBaseUserService<TUser, TIdentityUser, TIdentityRole>
    where TUser : class, IBaseUser<TIdentityUser, TIdentityRole>, new()
    where TIdentityUser : IdentityUser, new()
    where TIdentityRole : struct, Enum
{
    protected AuthenticationStateProvider AuthenticationStateProvider { get; }
    protected UserManager<TIdentityUser> UserManager { get; }
    protected BaseService BaseService { get; }

    public BaseUserService(AuthenticationStateProvider authenticationStateProvider, UserManager<TIdentityUser> userManager, BaseService baseService)
    {
        AuthenticationStateProvider = authenticationStateProvider;
        UserManager = userManager;
        BaseService = baseService;
    }

    public virtual async Task<bool> CurrentUserIsRoleAsync(string roleName)
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity.IsAuthenticated)
            return false;

        return authState.User.IsInRole(roleName);
    }

    public virtual async Task<TUser> GetCurrentUserAsync(BaseService baseService, bool asNoTracking = true)
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var userId = UserManager.GetUserId(authenticationState.User);
        return await GetUserByApplicationUserIdAsync(baseService, userId, asNoTracking);
    }

    public virtual Task<TUser> GetCurrentUserAsync(bool asNoTracking = true)
    {
        return GetCurrentUserAsync(BaseService, asNoTracking);
    }

    public virtual async Task<string> GetCurrentUserIdentityIdAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        return UserManager.GetUserId(authenticationState.User);
    }

    public virtual async Task<TUser> GetUserByApplicationUserIdAsync(BaseService baseService, string id, bool asNoTracking = true)
    {
        if (id == null)
            return null;

        var users = await baseService.GetDataAsync<TUser>(user => user.IdentityUserId == id, asNoTracking);

        return users.FirstOrDefault();
    }

    public virtual Task<TUser> GetUserByApplicationUserIdAsync(string id, bool asNoTracking = true)
    {
        return GetUserByApplicationUserIdAsync(BaseService, id, asNoTracking);
    }

    public static bool DatabaseHasNoUsers(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        return !userManager.Users.Any();
    }

    public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        var userRoles = Enum.GetNames<TIdentityRole>();

        foreach (var role in userRoles)
        {
            var result = await roleManager.FindByNameAsync(role);
            if (result == null)
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    public static async Task SeedUserAsync(IServiceProvider serviceProvider, string username, string email, string initPassword, TIdentityRole role)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetService<UserManager<TIdentityUser>>();

        if (await userManager.FindByEmailAsync(email) != null)
            return;

        var identityUser = new TIdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = userManager.CreateAsync(identityUser, initPassword).Result;

        if (result.Succeeded)
        {
            result = userManager.AddToRoleAsync(identityUser, role.ToString()).Result;
            if (!result.Succeeded)
                throw new Exception(result.GetErrorMessage());
        }
        else
            throw new Exception(result.GetErrorMessage());

        var baseService = scope.ServiceProvider.GetService<BaseService>();
        var user = new TUser
        {
            Id = baseService.GetNewPrimaryKey<TUser>(),
            Email = email,
            UserName = username,
            IdentityUserId = identityUser.Id,
            IdentityRole = role
        };

        baseService.AddEntry(user);
        await baseService.SaveChangesAsync();
    }
}
