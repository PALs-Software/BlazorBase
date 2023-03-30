using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.User.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBase.User.Models;

public partial class BaseUser : BaseUser<IdentityUser, BaseIdentityRole>
{
    protected override Task<bool> IdentityHasRightToChangeRoleAsync(ClaimsPrincipal currentLoggedInUser, BaseIdentityRole identityChangedRole, IdentityUser? identityToChange)
    {
        return Task.FromResult(currentLoggedInUser.IsInRole(BaseIdentityRole.Admin.ToString()));
    }
}

public abstract partial class BaseUser<TIdentityUser, TIdentityRole> : BaseModel, IBaseUser<TIdentityUser, TIdentityRole>
    where TIdentityUser : IdentityUser, new()
{
    #region Properties
    [Key]
    public Guid Id { get; set; }

    [Visible]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Visible]
    [Required]
    [DisplayKey]
    public string UserName { get; set; } = null!;

    [ForeignKey("IdentityUser")]
    public string? IdentityUserId { get; set; }
    public virtual TIdentityUser? IdentityUser { get; set; }

    [Visible]
    public TIdentityRole? IdentityRole { get; set; }
    #endregion

    #region CRUD
    public record OnBeforeCreateIdentityUserAsyncArgs(TIdentityUser IdentityUser, EventServices EventServices);
    public virtual Task OnBeforeCreateIdentityUserAsync(OnBeforeCreateIdentityUserAsyncArgs args) { return Task.CompletedTask; }
    public override async Task OnBeforeAddEntry(OnBeforeAddEntryArgs args)
    {
        var userManager = args.EventServices.ServiceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        await CheckIdentityRolePermissionsAsync(args.EventServices, userManager, null);

        Id = await args.EventServices.BaseService.GetNewPrimaryKeyAsync(GetType());

        IdentityUser = new TIdentityUser
        {
            Email = Email,
            UserName = UserName
        };

        await OnBeforeCreateIdentityUserAsync(new OnBeforeCreateIdentityUserAsyncArgs(IdentityUser, args.EventServices));
        var result = await userManager.CreateAsync(IdentityUser);

        if (!result.Succeeded)
            throw new CRUDException(String.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));

        IdentityUserId = IdentityUser.Id;
        var user = await userManager.FindByIdAsync(IdentityUser.Id);
        ArgumentNullException.ThrowIfNull(user);
        await SetIdentityRoleAsync(args.EventServices, userManager, user);
        await args.EventServices.BaseService.DbContext.Entry(IdentityUser).ReloadAsync();
    }

    public override async Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args)
    {
        ArgumentNullException.ThrowIfNull(IdentityUser);
        var userManager = args.EventServices.ServiceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        var user = await userManager.FindByIdAsync(IdentityUser.Id);
        ArgumentNullException.ThrowIfNull(user);

        if (IdentityUser.Email != Email)
        {
            var result = await userManager.ChangeEmailAsync(user, Email, await userManager.GenerateChangeEmailTokenAsync(user, Email));
            if (!result.Succeeded)
                throw new CRUDException(String.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));
        }

        if (IdentityUser.UserName != UserName)
        {
            var result = await userManager.SetUserNameAsync(user, UserName);
            if (!result.Succeeded)
                throw new CRUDException(String.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));
        }

        await SetIdentityRoleAsync(args.EventServices, userManager, user);
        await args.EventServices.BaseService.DbContext.Entry(IdentityUser).ReloadAsync();
    }

    protected abstract Task<bool> IdentityHasRightToChangeRoleAsync(ClaimsPrincipal currentLoggedInUser, TIdentityRole identityChangedRole, TIdentityUser? identityToChange);

    protected virtual async Task CheckIdentityRolePermissionsAsync(EventServices eventServices, UserManager<TIdentityUser> userManager, TIdentityUser? identityToChange)
    {
        var role = IdentityRole?.ToString();
        ArgumentNullException.ThrowIfNull(IdentityRole);
        ArgumentNullException.ThrowIfNull(role);

        if (identityToChange != null && await userManager.IsInRoleAsync(identityToChange, role))
            return;

        var authenticationStateProvider = eventServices.ServiceProvider.GetRequiredService<AuthenticationStateProvider>();
        var currentLoggedInUser = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

        if (await IdentityHasRightToChangeRoleAsync(currentLoggedInUser, IdentityRole, identityToChange))
            return;

        throw new CRUDException(eventServices.Localizer["You have no permission to change the user role to {0}", eventServices.Localizer[role]]);
    }

    protected virtual async Task SetIdentityRoleAsync(EventServices eventServices, UserManager<TIdentityUser> userManager, TIdentityUser user)
    {
        var role = IdentityRole?.ToString();
        ArgumentNullException.ThrowIfNull(role);

        if (await userManager.IsInRoleAsync(user, role))
            return;

        await CheckIdentityRolePermissionsAsync(eventServices, userManager, user);

        var roles = await userManager.GetRolesAsync(user);

        var result = await userManager.RemoveFromRolesAsync(user, roles);
        if (!result.Succeeded)
            throw new CRUDException(String.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));

        result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new CRUDException(String.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));
    }

    public override async Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args)
    {
        var userManager = args.EventServices.ServiceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        var applicationUser = await userManager.FindByEmailAsync(Email);
        var result = IdentityResult.Success;
        if (applicationUser != null)
            result = await userManager.DeleteAsync(applicationUser);

        if (result.Succeeded)
        {
            IdentityUser = null;
            IdentityUserId = null;
        }
        else
            throw new CRUDException(String.Join(Environment.NewLine, result.Errors.Select(error => error.Description)));
    }
    #endregion
}
