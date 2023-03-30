using BlazorBase.User.Enums;
using BlazorBase.User.Models;
using BlazorBase.User.Services;
using BlazorBase.User.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public partial class BaseUserManagePassword : ComponentBase
{
    #region Parameter

    #endregion

    #region Inject
    [Inject] protected IStringLocalizer<BaseLoginForm> Localizer { get; set; } = null!;
    [Inject] protected SignInManager<IdentityUser> SignInManager { get; set; } = null!;
    [Inject] protected UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject]  protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] protected ILogger<BaseLoginForm> Logger { get; set; } = null!;
    #endregion

    #region Properties
    protected ChangePasswordData Data { get; set; } = new();
    protected string Feedback { get; set; } = String.Empty;
    #endregion

    #region Submit Login
    public async Task HandleValidSubmit()
    {
        Feedback = String.Empty;
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = await UserManager.GetUserAsync(authenticationState.User);
        if (user == null)
            return;

        var changePasswordResult = await UserManager.ChangePasswordAsync(user, Data.CurrentPassword, Data.NewPassword);
        if (changePasswordResult.Succeeded)
        {
            await SignInManager.RefreshSignInAsync(user);
            Logger.LogInformation("User changed their password successfully.");
            Feedback = Localizer["Your password has been changed."];
        }
        else
        {
            foreach (var error in changePasswordResult.Errors)
                Feedback += error.Description;
        }
    }
    #endregion

}
