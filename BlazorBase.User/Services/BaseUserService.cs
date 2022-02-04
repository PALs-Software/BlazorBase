using BlazorBase.CRUD.Services;
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

    public virtual async Task<TUser> GetCurrentUserAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var userId = UserManager.GetUserId(authenticationState.User);
        return await GetUserByApplicationUserIdAsync(userId);
    }

    public virtual async Task<TUser> GetUserByApplicationUserIdAsync(string id, bool asNoTracking = true)
    {
        if (id == null)
            return null;

        var users = await BaseService.GetDataAsync<TUser>(user => user.IdentityUserId == id, asNoTracking);

        return users.FirstOrDefault();
    }

    public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
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
        var userManager = serviceProvider.GetService<UserManager<TIdentityUser>>();
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
            userManager.AddToRoleAsync(identityUser, role.ToString()).Wait();

        var baseService = serviceProvider.GetService<BaseService>();
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
