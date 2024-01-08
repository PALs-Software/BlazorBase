using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.Services;

public class BaseAuthenticationService
{
    protected readonly AuthenticationStateProvider AuthenticationStateProvider;

    public BaseAuthenticationService(AuthenticationStateProvider authenticationStateProvider)
    {
        AuthenticationStateProvider = authenticationStateProvider;
    }

    public virtual async Task<List<string>> GetUserRolesAsync()
    {
        List<string> userRoles = new();
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
            userRoles.AddRange(authState.User.Claims.Where(claim => !String.IsNullOrEmpty(claim.Type) && claim.Type == claim.Subject?.RoleClaimType).Select(claim => claim.Value));

        return userRoles;
    }
}
