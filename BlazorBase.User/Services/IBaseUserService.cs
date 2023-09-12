using BlazorBase.CRUD.Services;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Identity;
using System;

using System.Threading.Tasks;

namespace BlazorBase.User.Services;

public interface IBaseUserService
{
    Task<bool> CurrentUserIsRoleAsync(string roleName);

    Task<IBaseUser?> GetCurrentUserAsync(BaseService baseService, bool asNoTracking = true);

    Task<IBaseUser?> GetCurrentUserAsync(bool asNoTracking = true);

    Task<IBaseUser?> GetUserByApplicationUserIdAsync(BaseService baseService, string id, bool asNoTracking = true);

    Task<IBaseUser?> GetUserByApplicationUserIdAsync(string id, bool asNoTracking = true);
}

public interface IBaseUserService<TUser, TIdentityUser, TIdentityRole>
    where TUser : IBaseUser<TIdentityUser, TIdentityRole>, new()
    where TIdentityUser : IdentityUser, new()
    where TIdentityRole : struct, Enum
{
    Task<bool> CurrentUserIsRoleAsync(string roleName);

    Task<TUser?> GetCurrentUserAsync(BaseService baseService, bool asNoTracking = true);

    Task<TUser?> GetCurrentUserAsync(bool asNoTracking = true);

    Task<TUser?> GetUserByApplicationUserIdAsync(BaseService baseService, string id, bool asNoTracking = true);

    Task<TUser?> GetUserByApplicationUserIdAsync(string id, bool asNoTracking = true);
}
