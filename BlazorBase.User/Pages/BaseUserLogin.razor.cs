using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.User.Pages;

public partial class BaseUserLogin : ComponentBase
{
    #region Inject
    [Inject] protected IStringLocalizer<BaseUserLogin> Localizer { get; set; } = null!;
    [Inject] protected IBlazorBaseUserOptions Options { get; set; } = null!;
    #endregion

    #region Member
    protected string? Password = null;
    #endregion
}
