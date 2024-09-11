using BlazorBase.User.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace BlazorBase.User.Pages;

public partial class BaseUserManage : ComponentBase
{
    #region Inject
    [Inject] protected IStringLocalizer<BaseUserManage> Localizer { get; set; } = null!;
    #endregion

    #region Members
    protected BaseUserManageProfile? UserManageProfile;
    #endregion

    protected Task<bool> OnUserProfileSaveClickedAsync()
    {
        if (UserManageProfile == null)
            return Task.FromResult(false);

        return UserManageProfile.SaveCardAsync();
    }
}
