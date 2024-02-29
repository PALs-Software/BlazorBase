using BlazorBase.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorBase.Server.Services;

public class BaseAuthenticationService(AuthenticationStateProvider authenticationStateProvider) : IBaseAuthenticationService
{
    protected readonly AuthenticationStateProvider AuthenticationStateProvider = authenticationStateProvider;

    public virtual async Task<List<string>> GetUserRolesAsync()
    {
        List<string> userRoles = new();
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
            userRoles.AddRange(authState.User.Claims.Where(claim => !String.IsNullOrEmpty(claim.Type) && claim.Type == claim.Subject?.RoleClaimType).Select(claim => claim.Value));

        return userRoles;
    }
}
