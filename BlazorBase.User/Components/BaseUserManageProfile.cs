using BlazorBase.CRUD.Components.Card;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public class BaseUserManageProfile : ComponentBase
{
    #region Inject
    [Inject] protected IBaseUser BaseUser { get; set; } = null!;
    #endregion

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.OpenComponent(0, typeof(BaseCard<>).MakeGenericType(BaseUser.GetType()));
        builder.AddAttribute(1, "Embedded", false);
        builder.AddAttribute(1, "ShowEntryByStart", true);
        builder.AddAttribute(1, "ShowActions", false);
        builder.AddAttribute(2, "EntryToBeShownByStart", new Func<OnEntryToBeShownByStartArgs, Task<IBaseModel?>>(GetShowOnlySingleEntryInstance));
        builder.CloseComponent();
    }

    public virtual async Task<IBaseModel?> GetShowOnlySingleEntryInstance(OnEntryToBeShownByStartArgs args)
    {
        args.ViewMode = true;

        return (IBaseModel?)await args.EventServices.DbContext.Set(BaseUser.GetType()).FirstOrDefaultTSAsync(args.EventServices.DbContext);
    }
}
