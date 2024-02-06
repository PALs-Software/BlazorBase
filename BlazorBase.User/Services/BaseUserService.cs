using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Services;
using BlazorBase.User.Extensions;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.User.Services;

public class BaseUserService<TUser, TIdentityUser, TIdentityRole>(
        AuthenticationStateProvider authenticationStateProvider,
        UserManager<TIdentityUser> userManager,
        IBaseDbContext dbContext) : IBaseUserService<TUser, TIdentityUser, TIdentityRole>, IBaseUserService
            where TUser : class, IBaseUser<TIdentityUser, TIdentityRole>, new()
            where TIdentityUser : IdentityUser, new()
            where TIdentityRole : struct, Enum
{
    protected AuthenticationStateProvider AuthenticationStateProvider { get; } = authenticationStateProvider;
    protected UserManager<TIdentityUser> UserManager { get; } = userManager;
    protected IBaseDbContext DbContext { get; } = dbContext;

    public virtual async Task<bool> CurrentUserIsInRoleAsync(string roleName)
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
        if (!authState.User.Identity?.IsAuthenticated ?? false)
            return false;

        return authState.User.IsInRole(roleName);
    }

    public virtual async Task<TUser?> GetCurrentUserAsync(IBaseDbContext dbContext, bool asNoTracking = true)
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
        var userId = UserManager.GetUserId(authenticationState.User);
        return await GetUserByApplicationUserIdAsync(dbContext, userId, asNoTracking).ConfigureAwait(false);
    }

    async Task<IBaseUser?> IBaseUserService.GetCurrentUserAsync(IBaseDbContext dbContext, bool asNoTracking)
    {
        return await GetCurrentUserAsync(dbContext, asNoTracking);
    }

    public virtual Task<TUser?> GetCurrentUserAsync(bool asNoTracking = true)
    {
        return GetCurrentUserAsync(DbContext, asNoTracking);
    }

    async Task<IBaseUser?> IBaseUserService.GetCurrentUserAsync(bool asNoTracking)
    {
        return await GetCurrentUserAsync(asNoTracking);
    }

    public virtual async Task<string?> GetCurrentUserIdentityIdAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
        return UserManager.GetUserId(authenticationState.User);
    }

    public virtual async Task<TUser?> GetUserByApplicationUserIdAsync(IBaseDbContext dbContext, string? id, bool asNoTracking = true)
    {
        if (id == null)
            return null;

        var query = dbContext.Set<TUser>().Where(user => user.IdentityUserId == id);
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultTSAsync(dbContext).ConfigureAwait(false);
    }

    async Task<IBaseUser?> IBaseUserService.GetUserByApplicationUserIdAsync(IBaseDbContext dbContext, string id, bool asNoTracking)
    {
        return await GetUserByApplicationUserIdAsync(dbContext, id, asNoTracking);
    }


    public virtual Task<TUser?> GetUserByApplicationUserIdAsync(string id, bool asNoTracking = true)
    {
        return GetUserByApplicationUserIdAsync(DbContext, id, asNoTracking);
    }

    async Task<IBaseUser?> IBaseUserService.GetUserByApplicationUserIdAsync(string id, bool asNoTracking)
    {
        return await GetUserByApplicationUserIdAsync(id, asNoTracking);
    }

    public static bool DatabaseHasNoUsers(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        return !userManager.Users.Any();
    }

    public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userRoles = Enum.GetNames<TIdentityRole>();

        foreach (var role in userRoles)
        {
            var result = await roleManager.FindByNameAsync(role).ConfigureAwait(false);
            if (result == null)
                await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
        }
    }

    public static async Task SeedUserAsync(IServiceProvider serviceProvider, string username, string email, string initPassword, TIdentityRole role)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<TIdentityUser>>();

        if (await userManager.FindByEmailAsync(email).ConfigureAwait(false) != null)
            return;

        var identityUser = new TIdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(identityUser, initPassword).ConfigureAwait(false);

        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(identityUser, role.ToString()).ConfigureAwait(false);
            if (!result.Succeeded)
                throw new Exception(result.GetErrorMessage());
        }
        else
            throw new Exception(result.GetErrorMessage());

        var dbContext = serviceProvider.GetRequiredService<IBaseDbContext>();
        var user = new TUser
        {
            Id = await dbContext.GetNewPrimaryKeyTSAsync<TUser>().ConfigureAwait(false),
            Email = email,
            UserName = username,
            IdentityUserId = identityUser.Id,
            IdentityRole = role
        };

        await dbContext.AddAsync(user).ConfigureAwait(false);
        await dbContext.SaveChangesTSAsync().ConfigureAwait(false);
    }

}
