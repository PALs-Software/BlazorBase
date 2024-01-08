#nullable enable

using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.EventArguments;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models;

public class ExtendedInformationBaseModel : BaseModel
{
    #region Properties

    [Editable(false)]
    [Visible(DisplayGroup = "Information", DisplayGroupOrder = 9999, DisplayOrder = 150, Collapsed = true)]
    public virtual string? CreatedBy { get; set; }

    [Editable(false)]
    [Visible(DisplayGroup = "Information", DisplayOrder = 250)]
    public virtual string? LastModifiedBy { get; set; }

    #endregion

    public override async Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args)
    {
        await base.OnBeforeDbContextAddEntry(args);

        var userName = await GetCurrentUserNameAsync(args.EventServices.ServiceProvider);
        if (userName != null)
        {
            if (String.IsNullOrEmpty(CreatedBy))
                CreatedBy = userName;

            LastModifiedBy = userName;
        }
    }

    public override async Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args)
    {
        await base.OnBeforeDbContextModifyEntry(args);
        var userName = await GetCurrentUserNameAsync(args.EventServices.ServiceProvider);
        if (userName != null)
            LastModifiedBy = userName;
    }

    protected async Task<string?> GetCurrentUserNameAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var authService = serviceProvider.GetService<AuthenticationStateProvider>();
            if (authService == null)
                return null;

            var authState = await authService.GetAuthenticationStateAsync();

            if (!String.IsNullOrEmpty(authState.User.Identity?.Name))
                return authState.User.Identity.Name;
        }
        catch (Exception) { } // If GetAuthenticationStateAsync is called in a non-user session, such as through a Web service request, it throws an error

        return null;
    }
}
