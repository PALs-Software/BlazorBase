using BlazorBase.Models;
using BlazorBase.User.Controller;
using System;
using System.Security;

namespace BlazorBase.User.Models;
public class BlazorBaseUserOptions : IBlazorBaseUserOptions
{
    #region Constructors
    public BlazorBaseUserOptions() { }
    public BlazorBaseUserOptions(IServiceProvider serviceProvider, Action<BlazorBaseUserOptions> configureOptions)
    {
        (this as IBlazorBaseUserOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties
    public BaseOptionsImportMode OptionsImportMode { get; set; }

    public string LoginPath { get; set; } = "User/Login";
    public string LoginControllerPath { get; set; } = UserLoginController.LoginPath;
    public string LogoutControllerPath { get; set; } = UserLoginController.LogoutPath;
    public string ManageUserPath { get; set; } = "User/Manage";
    public string WebsiteName { get; set; } = "BlazorBase";
    public bool ShowImageOfTheDayAsBackgroundImageByLogin { get; set; } = true;
    #endregion


}
