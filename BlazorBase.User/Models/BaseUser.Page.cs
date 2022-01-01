using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.User.Services;
using BlazorBase.Extensions;
using BlazorBase.Mailing.Services;
using BlazorBase.User.Enums;
using System.Runtime.Versioning;

namespace BlazorBase.User.Models;

public abstract partial class BaseUser<TIdentityUser, TIdentityRole> : BaseModel where TIdentityUser : IdentityUser, new()
{
    [SupportedOSPlatform("windows")]
    public override List<PageActionGroup> GeneratePageActionGroups()
    {
        return new List<PageActionGroup>()
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
                            Action = async (eventServices, model) =>
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
                            Action = async (eventServices, model) =>
                            {
                                if (model is not BaseUser user)
                                    return;

                                await TestModelIsInValidStateForCallingActionAsync(eventServices);
                                if (await user.SendPasswordMailAsync(eventServices, UserMailTemplate.ResetUserPassword))
                                    eventServices.MessageHandler.ShowMessage(eventServices.Localizer["Reset Password"],
                                                                            eventServices.Localizer["The password reset email was sent successfully"]);
                            }
                        }
                    }
                }
            };
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
        var messageId = eventServices.MessageHandler.ShowLoadingMessage(eventServices.Localizer["Sending e-mail..."]);

        var serviceProvider = eventServices.ServiceProvider;
        var userManager = serviceProvider.GetService<UserManager<TIdentityUser>>();
        var accessor = serviceProvider.GetService<IHttpContextAccessor>();
        var linkGenerator = serviceProvider.GetService<LinkGenerator>();
        var mailService = serviceProvider.GetService<BaseMailService<UserMailTemplate>>();

        var user = await userManager.FindByIdAsync(IdentityUserId);
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = linkGenerator.GetUriByPage(accessor.HttpContext,
                                                     page: "/Account/ResetPassword",
                                                     handler: null,
                                                     values: new { area = "Identity", code });
        callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

        var success = await mailService.SendMailAsync(user.Email,
                                                        mailTemplate,
                                                        Array.Empty<string>(),
                                                        new string[] { user.UserName, callbackUrl });

        eventServices.MessageHandler.CloseLoadingMessage(messageId);
        return success;
    }
}
