using BlazorBase.Models;

namespace BlazorBase.User.Models;
public interface IBlazorBaseUserOptions : IBaseOptions
{
    string LoginPath { get; set; }
    string LoginControllerPath { get; set; }
    string LogoutControllerPath { get; set; }
    string IdentityAccessDeniedPath { get; set; }
    string ManageUserPath { get; set; }
    string? WebsiteName { get; set; }
    bool ShowImageOfTheDayAsBackgroundImageByLogin { get; set; }
    string? LoginBackgroundImageSrc { get; set; }

    bool LogUserSessions { get; set; }
}