using BlazorBase.CRUD.Components.PageActions.Models;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Extensions;
using BlazorBase.Mailing.Services;
using BlazorBase.User.Enums;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BlazorBase.User.Models;

public abstract partial class BaseUser<TIdentityUser, TIdentityRole> : BaseModel where TIdentityUser : IdentityUser, new()
{
    [SupportedOSPlatform("windows")]
    public override Task<List<PageActionGroup>?> GeneratePageActionGroupsAsync(EventServices eventServices)
    {
        return Task.FromResult<List<PageActionGroup>?>(new List<PageActionGroup>()
        {
            new PageActionGroup()
            {
                Caption = PageActionGroup.DefaultGroups.Process,
                Image = FontAwesomeIcons.Bolt,
                VisibleInGUITypes = new GUIType[] { GUIType.Card },
                PageActions = new List<PageAction>()
                {
                    new PageAction()
                    {
                        Caption = "Send account setup email",
                        ToolTip = "Sends the user an email to set up his account with an password",
                        Image = FontAwesomeIcons.Cogs,
                        Visible = eventServices => Task.FromResult(true),
                        VisibleInGUITypes = new GUIType[] { GUIType.Card },
                        Action = async (source, eventServices, model) =>
                        {
                            if (model is not BaseUser<TIdentityUser, TIdentityRole> user)
                                return;

                            await TestModelIsInValidStateForCallingActionAsync(eventServices);
                            if (await user.SendPasswordMailAsync(eventServices, UserMailTemplate.SetupUser))
                                eventServices.MessageHandler.ShowMessage(eventServices.Localizer["Setup email"],
                                                                        eventServices.Localizer["The setup email was sent successfully"]);
                        }
                    },
                    new PageAction()
                    {
                        Caption = "Reset Password",
                        ToolTip = "Sends the user an email to reset the password",
                        Image = FontAwesomeIcons.Unlock,
                        Visible = eventServices => Task.FromResult(true),
                        VisibleInGUITypes = new GUIType[] { GUIType.Card },
                        Action = async (source, eventServices, model) =>
                        {
                            if (model is not BaseUser<TIdentityUser, TIdentityRole> user)
                                return;

                            await TestModelIsInValidStateForCallingActionAsync(eventServices);
                            if (await user.SendPasswordMailAsync(eventServices, UserMailTemplate.ResetUserPassword))
                                eventServices.MessageHandler.ShowMessage(eventServices.Localizer["Reset Password"],
                                                                        eventServices.Localizer["The password reset email was sent successfully"]);
                        }
                    }
                }
            }
        });
    }

    protected virtual async Task TestModelIsInValidStateForCallingActionAsync(EventServices services)
    {
        if (!TryValidate(out List<ValidationResult> results, services))
            throw new CRUDException(services.Localizer["The user card cannot be saved to call actions: {0}", Environment.NewLine + Environment.NewLine + results.FormatResultsToString()]);

        await services.BaseService.SaveChangesAsync();
    }

    [SupportedOSPlatform("windows")]
    public async Task<bool> SendPasswordMailAsync(EventServices eventServices, UserMailTemplate mailTemplate)
    {
        if (IdentityUserId == null)
            return false;

        var messageId = eventServices.MessageHandler.ShowLoadingMessage(eventServices.Localizer["Sending e-mail..."]);

        var serviceProvider = eventServices.ServiceProvider;
        var userManager = serviceProvider.GetRequiredService<UserManager<TIdentityUser>>();
        var accessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var linkGenerator = serviceProvider.GetRequiredService<LinkGenerator>();
        var mailService = serviceProvider.GetRequiredService<BaseMailService<UserMailTemplate>>();

        var user = await userManager.FindByIdAsync(IdentityUserId);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(user.Email);
        ArgumentNullException.ThrowIfNull(user.UserName);

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        ArgumentNullException.ThrowIfNull(accessor.HttpContext);
        var callbackUrl = linkGenerator.GetUriByPage(accessor.HttpContext,
                                                     page: "/Account/ResetPassword",
                                                     handler: null,
                                                     values: new { area = "Identity", code });
        ArgumentNullException.ThrowIfNull(callbackUrl);
        callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

        var success = await mailService.SendMailAsync(user.Email,
                                                        mailTemplate,
                                                        Array.Empty<string>(),
                                                        new string[] { user.UserName, callbackUrl });

        eventServices.MessageHandler.CloseLoadingMessage(messageId);
        return success;
    }
}
