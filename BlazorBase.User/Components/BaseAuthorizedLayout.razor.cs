using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public partial class BaseAuthorizedLayout
{
    #region Injects
    [Inject] protected IStringLocalizer<BaseAuthorizedLayout> Localizer { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected IBlazorBaseUserOptions Options { get; set; } = null!;
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationState { get; set; } = null!;
    #endregion

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var authState = await AuthenticationState;
        if (authState.User.Identity?.IsAuthenticated ?? false)
            return;

        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        NavigationManager.NavigateTo($"{Options.LoginPath}?returnUrl={uri.PathAndQuery}", true);
    }
}
