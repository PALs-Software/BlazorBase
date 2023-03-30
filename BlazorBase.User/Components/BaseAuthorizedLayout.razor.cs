using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public partial class BaseAuthorizedLayout
{
    #region Injects
    [Inject] protected IStringLocalizer<BaseAuthorizedLayout> Localizer { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; }
    [Inject] protected IBlazorBaseUserOptions Options { get;set;}
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationState { get; set; }
    #endregion

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var authState = await AuthenticationState;
        if (authState.User.Identity.IsAuthenticated)
            return;

        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        NavigationManager.NavigateTo($"{Options.LoginPath}?returnUrl={uri.PathAndQuery}", true);
    }
}
