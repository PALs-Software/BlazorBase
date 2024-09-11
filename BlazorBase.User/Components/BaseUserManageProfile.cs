using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.User.Models;
using BlazorBase.User.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public class BaseUserManageProfile : ComponentBase
{
    #region Inject
    [Inject] protected IBaseUser BaseUser { get; set; } = null!;
    [Inject] protected IBaseUserService UserService { get; set; } = null!;
    #endregion

    #region Members
    protected IBaseCard? BaseCard;
    #endregion

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.OpenComponent(0, typeof(BaseCard<>).MakeGenericType(BaseUser.GetType()));
        builder.AddAttribute(1, "Embedded", true);
        builder.AddAttribute(2, "ShowEntryByStart", true);
        builder.AddAttribute(3, "ShowActions", false);
        builder.AddAttribute(4, "EntryToBeShownByStart", new Func<OnEntryToBeShownByStartArgs, Task<IBaseModel?>>(GetShowOnlySingleEntryInstance));
        builder.AddComponentReferenceCapture(1000, (card) => BaseCard = (IBaseCard?)card);

        builder.CloseComponent();
    }

    public virtual async Task<IBaseModel?> GetShowOnlySingleEntryInstance(OnEntryToBeShownByStartArgs args)
    {
        return await UserService.GetCurrentUserAsync(args.EventServices.DbContext);
    }

    public async Task<bool> SaveCardAsync()
    {
        if (BaseCard == null)
            return false;

        var result = await BaseCard.SaveCardAsync();
        _ = InvokeAsync(StateHasChanged);

        return result;
    }
}
