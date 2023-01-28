using BlazorBase.Models;
using System.Security;

namespace BlazorBase.User.Models;
public interface IBlazorBaseUserOptions : IBaseOptions
{
    string LoginPath { get; set; }
    string LoginControllerPath { get; set; }
    string LogoutControllerPath { get; set; }
    string IdentityAccessDeniedPath { get; set; }
    string ManageUserPath { get; set; }
    string WebsiteName { get; set; }
    bool ShowImageOfTheDayAsBackgroundImageByLogin { get; set; }
}