using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.User.Pages;

public partial class BaseUserManage : ComponentBase
{
    #region Inject
    [Inject] protected IStringLocalizer<BaseUserManage> Localizer { get; set; } = null!;
    #endregion

}
