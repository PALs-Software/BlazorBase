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
    public virtual string CreatedBy { get; set; }

    [Editable(false)]
    [Visible(DisplayGroup = "Information", DisplayOrder = 250)]
    public virtual string LastModifiedBy { get; set; }

    #endregion

    public override async Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args)
    {
        await base.OnBeforeDbContextAddEntry(args);
        await FillModifiedByWithCurrentUserNameAsync(args.EventServices.ServiceProvider);
    }

    public override async Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args)
    {
        await base.OnBeforeDbContextModifyEntry(args);
        await FillModifiedByWithCurrentUserNameAsync(args.EventServices.ServiceProvider);
    }

    protected async Task FillModifiedByWithCurrentUserNameAsync(IServiceProvider serviceProvider)
    {
        var authService = serviceProvider.GetService<AuthenticationStateProvider>();
        if (authService == null)
            return;

        var authState = await authService.GetAuthenticationStateAsync();
        if (String.IsNullOrEmpty(authState.User.Identity?.Name))
            return;

        LastModifiedBy = authState.User.Identity.Name;
    }
}
