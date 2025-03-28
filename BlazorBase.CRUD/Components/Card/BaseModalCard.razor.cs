﻿using BlazorBase.CRUD.Components.Modals;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.Helper;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.Card;

public partial class BaseModalCard<TModel> where TModel : class, IBaseModel, new()
{
    #region Parameter

    #region Events
    [Parameter] public EventCallback<OnGetPropertyCaptionArgs> OnGetPropertyCaption { get; set; }
    [Parameter] public EventCallback<OnGuiLoadDataArgs> OnGuiLoadData { get; set; }
    [Parameter] public EventCallback<OnShowEntryArgs> OnShowEntry { get; set; }
    [Parameter] public EventCallback OnCardClosed { get; set; }
    [Parameter] public EventCallback<OnCreateNewEntryInstanceArgs> OnCreateNewEntryInstance { get; set; }
    [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
    [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
    [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnBeforeCardSaveChangesArgs> OnBeforeSaveChanges { get; set; }
    [Parameter] public EventCallback<OnAfterCardSaveChangesArgs> OnAfterSaveChanges { get; set; }

    [Parameter] public EventCallback<OnAfterGetVisiblePropertiesArgs> OnAfterGetVisibleProperties { get; set; }
    [Parameter] public EventCallback<OnAfterSetUpDisplayListsArgs> OnAfterSetUpDisplayLists { get; set; }

    #region List Events
    [Parameter] public EventCallback<OnCreateNewListEntryInstanceArgs> OnCreateNewListEntryInstance { get; set; }
    [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
    [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertListPropertyTypeArgs> OnBeforeConvertListPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforeListPropertyChangedArgs> OnBeforeListPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterListPropertyChangedArgs> OnAfterListPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnBeforeRemoveListEntryArgs> OnBeforeRemoveListEntry { get; set; }
    [Parameter] public EventCallback<OnAfterRemoveListEntryArgs> OnAfterRemoveListEntry { get; set; }
    [Parameter] public EventCallback<OnAfterMoveListEntryUpArgs> OnAfterMoveListEntryUp { get; set; }
    [Parameter] public EventCallback<OnAfterMoveListEntryDownArgs> OnAfterMoveListEntryDown { get; set; }
    #endregion

    #endregion

    [Parameter] public string? SingleDisplayName { get; set; }
    [Parameter] public ExplainText? ExplainText { get; set; }
    [Parameter] public string? CloseButtonText { get; set; }
    [Parameter] public IBaseDbContext? ParentDbContext { get; set; }
    [Parameter] public bool ShowEntryByStart { get; set; }
    [Parameter] public Func<OnEntryToBeShownByStartArgs, Task<IBaseModel>>? EntryToBeShownByStart { get; set; }
    [Parameter] public TModel? ComponentModelInstance { get; set; }
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public RenderFragment<AdditionalHeaderPageActionsArgs> AdditionalHeaderPageActions { get; set; } = null!;

    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseCard<TModel>> Localizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    #endregion

    #region Properties
    public TModel? CurrentModelInstance { get { return BaseCard?.CurrentModelInstance; } }
    #endregion

    #region Member
    protected BaseModal? Modal = null;
    protected BaseCard<TModel>? BaseCard = null;
    protected bool ContinueByUnsavedChanges = false;
    protected bool ViewMode = false;

    protected string Title = String.Empty;
    #endregion

    #region Init

    protected override Task OnInitializedAsync()
    {
        if (String.IsNullOrEmpty(SingleDisplayName))
            SingleDisplayName = ModelLocalizer[typeof(TModel).Name];
        else
            SingleDisplayName = ModelLocalizer[SingleDisplayName];

        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && ShowEntryByStart)
            Modal?.Show();

        base.OnAfterRender(firstRender);
    }

    #endregion

    public async Task ShowModalAsync(bool addingMode = false, bool viewMode = false, object?[]? primaryKeys = null, TModel? template = null)
    {
        ContinueByUnsavedChanges = false;
        ViewMode = viewMode;

        await (BaseCard?.ShowAsync(addingMode, viewMode, primaryKeys, template) ?? Task.CompletedTask);
        Modal?.Show();
    }

    public virtual async Task ReloadEntityFromDatabase()
    {
        await (BaseCard?.ReloadEntityFromDatabase() ?? Task.CompletedTask);
    }

    public async Task<bool> SaveModalAsync()
    {
        return await (BaseCard?.SaveCardAsync() ?? Task.FromResult(false));
    }

    public async Task SaveAndCloseModalAsync()
    {
        if (await SaveModalAsync())
            await HideModalAsync();
    }

    public Task HideModalAsync()
    {
        if (Modal == null)
            return Task.CompletedTask;

        return Modal.HideAsync();
    }

    protected async Task OnModalClosing(ModalClosingEventArgs args)
    {
        if (BaseCard == null)
        {
            await OnCardClosed.InvokeAsync(null);
            return;
        }

        var cardIsValid = await BaseCard.CardIsValidAsync(); // Trigger validation and also get all changed informations from inputs to the model
        if (ParentDbContext == null)
        {
            if (!ContinueByUnsavedChanges && await HasUnsavedChangesAsync())
            {
                args.Cancel = true;
                return;
            }
        }
        else if (!cardIsValid)
        {
            _ = InvokeAsync(StateHasChanged);
            args.Cancel = true;
            return;
        }

        BaseCard.ResetCard();
        await OnCardClosed.InvokeAsync(null);
    }

    protected async Task<bool> HasUnsavedChangesAsync()
    {
        if (BaseCard == null)
            return false;

        if (!await BaseCard.HasUnsavedChangesAsync())
            return false;

        MessageHandler.ShowConfirmDialog(
            Localizer["Unsaved changes"],
            Localizer["There are currently unsaved changes, these will be lost when you leave the card, continue anyway?"],
            confirmButtonText: Localizer["Leave Card"],
            confirmButtonColor: Color.Secondary,
            abortButtonText: Localizer["Abort"],
            abortButtonColor: Color.Primary,
            onClosing: (closingArgs, result) => UserHandleUnsavedChangesConfirmDialogAsync(result));

        return true;
    }

    protected virtual async Task UserHandleUnsavedChangesConfirmDialogAsync(ConfirmDialogResult result)
    {
        if (result == ConfirmDialogResult.Aborted)
            return;

        ContinueByUnsavedChanges = true;
        await HideModalAsync();
    }

    protected virtual void OnTitleCalculated(string title)
    {
        Title = title;
        InvokeAsync(StateHasChanged);
    }

    protected virtual Task GetVisiblePropertiesAsync(OnAfterGetVisiblePropertiesArgs args)
    {
        return OnAfterGetVisibleProperties.InvokeAsync(args);
    }

    public bool? CardIsInAddingMode()
    {
        return BaseCard?.CardIsInAddingMode();
    }

    public bool? CardIsInViewMode()
    {
        return BaseCard?.CardIsInViewMode();
    }
}