using BlazorBase.CRUD.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.SelectList;

public class BaseTypeBasedSelectList : ComponentBase, IBaseSelectList
{
    #region Parameters
    [Parameter] public Type BaseModelType { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string SelectButtonText { get; set; }
    [Parameter] public bool HideSelectButton { get; set; } = false;
    [Parameter] public EventCallback<OnSelectListClosedArgs> OnSelectListClosed { get; set; }
    #endregion

    #region Records
    public record OnSelectListClosedArgs(ModalClosingEventArgs ModalClosingEventArgs, IBaseModel SelectedModel);
    #endregion

    #region Member
    protected IBaseSelectList BaseSelectList;
    #endregion

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.OpenComponent(0, typeof(BaseSelectList<>).MakeGenericType(BaseModelType));
        builder.AddAttribute(1, "Title", Title);
        builder.AddAttribute(2, "SelectButtonText", SelectButtonText);
        builder.AddAttribute(3, "HideSelectButton", HideSelectButton);
        builder.AddAttribute(4, "OnSelectListClosed", EventCallback.Factory.Create<OnSelectListClosedArgs>(this, (args) => OnSelectListClosed.InvokeAsync(args)));

        builder.AddComponentReferenceCapture(5, (value) => BaseSelectList = (IBaseSelectList)value);

        builder.CloseComponent();
    }

    public Task ShowModalAsync()
    {
        return BaseSelectList.ShowModalAsync();
    }

    public void HideModal()
    {
        BaseSelectList.HideModal();
    }

    public IBaseModel GetSelectedEntry()
    {
        return BaseSelectList.GetSelectedEntry();
    }
}
