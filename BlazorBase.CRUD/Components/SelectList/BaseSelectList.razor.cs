using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Blazorise;
using System;
using static BlazorBase.CRUD.Components.SelectList.BaseTypeBasedSelectList;
using System.Linq.Expressions;

namespace BlazorBase.CRUD.Components.SelectList;

public partial class BaseSelectList<TModel> : ComponentBase, IBaseSelectList where TModel : class, IBaseModel, new()
{
    #region Parameters
    [Parameter] public string Title { get; set; }
    [Parameter] public string SelectButtonText { get; set; }
    [Parameter] public bool HideTitle { get; set; } = false;
    [Parameter] public bool HideSelectButton { get; set; } = false;
    [Parameter] public virtual Expression<Func<IBaseModel, bool>> DataLoadCondition { get; set; }
    [Parameter] public bool RenderAdditionalActionsOutsideOfButtonGroup { get; set; } = false;
    [Parameter] public RenderFragment<TModel> AdditionalActions { get; set; } = null;
    [Parameter] public EventCallback<OnSelectListClosedArgs> OnSelectListClosed { get; set; }
    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseSelectList<TModel>> SelectListLocalizer { get; set; }
    [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; }
    #endregion

    #region Members
    protected bool CurrentlyVisible = false;
    protected Modal Modal = default!;
    protected TModel SelectedEntry = null;

    protected object AdditionalData = null;
    #endregion

    #region Init
    protected override Task OnInitializedAsync()
    {
        var modelType = typeof(TModel);
        if (String.IsNullOrEmpty(Title))
            Title = ModelLocalizer[$"{modelType.Name}_Plural"];

        if (HideSelectButton)
            RenderAdditionalActionsOutsideOfButtonGroup = true;

        return base.OnInitializedAsync();
    }
    #endregion

    protected void SelectEntry(TModel entry)
    {
        SelectedEntry = entry;
        HideModal();
    }

    public IBaseModel GetSelectedEntry()
    {
        return SelectedEntry;
    }

    public void ShowModal(object additionalData = null)
    {
        AdditionalData = additionalData;
        CurrentlyVisible = true;
        SelectedEntry = null;
        Modal.Show();
    }

    public void HideModal()
    {
        Modal.Hide();
        CurrentlyVisible = false;
    }

    public void OnModalClosing(ModalClosingEventArgs args)
    {
        InvokeAsync(async () => await OnSelectListClosed.InvokeAsync(new OnSelectListClosedArgs(args, SelectedEntry, AdditionalData)));
    }

}