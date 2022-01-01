using BlazorBase.CRUD.Services;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.User.Services;

public class BaseUserService<TIdentityUser> where TIdentityUser : IdentityUser, new()
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

    public async Task<bool> UserIsAdminAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity.IsAuthenticated)
            return false;

        return authState.User.IsInRole(Enums.BaseIdentityRole.Admin.ToString());
    }

    public async Task<BaseUser> GetCurrentUserAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var userId = UserManager.GetUserId(authenticationState.User);
        return await GetUserByApplicationUserIdAsync(userId);
    }

    public async Task<BaseUser> GetUserByApplicationUserIdAsync(string id)
    {
        if (id == null)
            return null;

        return await BaseService.Set<BaseUser>()
            .Where(user => user.ApplicationUserId == id)
            .Include(user => user.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
