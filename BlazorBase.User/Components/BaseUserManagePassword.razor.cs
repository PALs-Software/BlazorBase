using BlazorBase.User.Attributes;
using BlazorBase.User.Models;
using BlazorBase.User.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public partial class BaseUserManagePassword : ComponentBase
{
    #region Parameter

    #endregion

    #region Inject
    [Inject] protected IStringLocalizer<BaseUserManagePassword> Localizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<IdentityPasswordLengthValidationAttribute> PasswordValidationLocalizer { get; set; } = null!;
    [Inject] protected SignInManager<IdentityUser> SignInManager { get; set; } = null!;
    [Inject] protected UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] protected IBlazorBaseUserOptions Options { get; set; } = null!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] protected ILogger<BaseLoginForm> Logger { get; set; } = null!;
    #endregion

    #region Properties
    protected ChangePasswordData Data { get; set; } = new();
    protected string Feedback { get; set; } = String.Empty;
    #endregion

    #region Submit
    public async Task OnSubmit()
    {
        Feedback = String.Empty;
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = await UserManager.GetUserAsync(authenticationState.User);
        if (user == null)
            return;

        if (String.IsNullOrEmpty(Data.CurrentPassword))
        {
            Feedback = Localizer["The \"Current Password\" field is required."];
            return;
        }

        if (String.IsNullOrEmpty(Data.NewPassword))
        {
            Feedback = Localizer["The \"New Password\" field is required."];
            return;
        }

        if (Data.NewPassword != Data.ConfirmPassword)
        {
            Feedback = Localizer["• The new password and confirmation password do not match"];
            return;
        }

        var changePasswordResult = await UserManager.ChangePasswordAsync(user, Data.CurrentPassword, Data.NewPassword);
        if (changePasswordResult.Succeeded)
        {
            Logger.LogInformation("User changed their password successfully.");

            // Needed because SignInManager.RefreshSignInAsync is not possible over Blazor Session, so post values also to a standard controller
            await JSRuntime.InvokeVoidAsync("blazorBase.user.submitForm", "logout-form");
        }
        else
        {
            List<string> errors = new();
            foreach (var error in changePasswordResult.Errors)
            {
                string errorMessage = error.Code switch
                {
                    nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => (string)PasswordValidationLocalizer[error.Code, UserManager.Options.Password.RequiredUniqueChars],
                    nameof(IdentityErrorDescriber.PasswordTooShort) => (string)PasswordValidationLocalizer[error.Code, UserManager.Options.Password.RequiredLength],
                    _ => (string)PasswordValidationLocalizer[error.Code],
                };
                errors.Add(errorMessage);
            }

            Feedback += String.Join(Environment.NewLine, errors);
        }
    }
    #endregion

}
