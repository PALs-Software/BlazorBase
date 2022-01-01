using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.User.Pages;

public partial class BaseAccountManage : ComponentBase
{
    #region Inject
    [Inject] protected IStringLocalizer<BaseAccountManage> Localizer { get; set; }
    #endregion

    #region Member
    protected string Password = null;
    #endregion
}
