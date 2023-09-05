using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace BlazorBase.User.Pages;

public partial class BaseUserManage : ComponentBase
{
    #region Inject
    [Inject] protected IStringLocalizer<BaseUserManage> Localizer { get; set; } = null!;
    #endregion

}
