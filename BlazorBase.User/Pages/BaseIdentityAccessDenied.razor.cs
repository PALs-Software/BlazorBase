using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.User.Pages;

public partial class BaseIdentityAccessDenied : ComponentBase
{
    #region Inject
    [Inject] protected IStringLocalizer<BaseIdentityAccessDenied> Localizer { get; set; }
    #endregion

}
