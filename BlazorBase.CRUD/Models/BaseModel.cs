using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.CRUD.Components.List;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models;

public class BaseModel : Abstractions.CRUD.Models.BaseModel
{
    protected override void BuildListComponent(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, typeof(BaseList<>).MakeGenericType(GetType()));
        builder.AddAttribute(1, "UserCanAddEntries", UserCanAddEntries);
        builder.AddAttribute(2, "UserCanEditEntries", UserCanEditEntries);
        builder.AddAttribute(3, "UserCanOpenCardReadOnly", UserCanOpenCardReadOnly);
        builder.AddAttribute(4, "UserCanDeleteEntries", UserCanDeleteEntries);

        builder.AddAttribute(5, "StickyRowButtons", StickyRowButtons);

        builder.AddAttribute(6, "DataLoadConditions", DataLoadConditions);
        builder.AddAttribute(7, "ComponentModelInstance", this);
        builder.CloseComponent();
    }

    protected override void BuildCardComponent(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, typeof(BaseCard<>).MakeGenericType(GetType()));
        builder.AddAttribute(1, "ShowEntryByStart", ShowOnlySingleEntry);
        builder.AddAttribute(2, "EntryToBeShownByStart", new Func<OnEntryToBeShownByStartArgs, Task<IBaseModel?>>(GetShowOnlySingleEntryInstance));
        builder.CloseComponent();
    }
}
