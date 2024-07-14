using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.CRUD.Models;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.Card;

public partial class BaseModalCard<TModel> : IBaseModalCard where TModel : class, IBaseModel, new()
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
    [Parameter] public bool ShowEntryByStart { get; set; }
    [Parameter] public Func<OnEntryToBeShownByStartArgs, Task<IBaseModel>>? EntryToBeShownByStart { get; set; }
    [Parameter] public TModel? ComponentModelInstance { get; set; }
    [Parameter] public bool ShowActions { get; set; } = true;

    #region Custom Render
    [Parameter] public Type? CustomBaseCardType { get; set; } = null;
    [Parameter] public RenderFragment<AdditionalHeaderPageActionsArgs> AdditionalHeaderPageActions { get; set; } = null!;
    #endregion

    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseCard<TModel>> Localizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    #endregion

    #region Properties
    public TModel? CurrentModelInstance { get { return (TModel?)BaseCard?.CurrentBaseModelInstance; } }
    #endregion

    #region Member
    protected Modal? Modal = null;
    protected IBaseCard? BaseCard = null;
    protected bool ContinueByUnsavedChanges = false;
    protected bool ViewMode = false;

    protected string Title = String.Empty;

    protected RenderFragment? BaseCardRenderFragment;
    #endregion

    #region Init

    protected override Task OnInitializedAsync()
    {
        if (String.IsNullOrEmpty(SingleDisplayName))
            SingleDisplayName = ModelLocalizer[typeof(TModel).Name];
        else
            SingleDisplayName = ModelLocalizer[SingleDisplayName];

        if (CustomBaseCardType == null)
            BaseCardRenderFragment = CreateBaseCardRenderFragment(typeof(BaseCard<TModel>));
        else
            BaseCardRenderFragment = CreateBaseCardRenderFragment(CustomBaseCardType);

        return base.OnInitializedAsync();
    }

    protected virtual RenderFragment CreateBaseCardRenderFragment(Type type) => builder =>
    {
        builder.OpenComponent(0, type);

        builder.AddAttribute(1, "SingleDisplayName", SingleDisplayName);
        builder.AddAttribute(2, "Embedded", true);
        builder.AddAttribute(3, "ShowActions", ShowActions);
        builder.AddAttribute(4, "ShowEntryByStart", ShowEntryByStart);
        builder.AddAttribute(5, "ComponentModelInstance", ComponentModelInstance);
        builder.AddAttribute(6, "EntryToBeShownByStart", EntryToBeShownByStart);
        builder.AddAttribute(7, "OnShowEntry", OnShowEntry);
        builder.AddAttribute(8, "AdditionalHeaderPageActions", AdditionalHeaderPageActions);
        builder.AddAttribute(9, "OnTitleCalculated", EventCallback.Factory.Create<string>(this, OnTitleCalculated));

        builder.AddAttribute(100, "OnAfterGetVisibleProperties", EventCallback.Factory.Create<OnAfterGetVisiblePropertiesArgs>(this, GetVisiblePropertiesAsync));
        builder.AddAttribute(110, "OnAfterSetUpDisplayLists", OnAfterSetUpDisplayLists);
        builder.AddAttribute(120, "OnCreateNewEntryInstance", OnCreateNewEntryInstance);
        builder.AddAttribute(130, "OnGuiLoadData", OnGuiLoadData);
        builder.AddAttribute(140, "OnBeforeAddEntry", OnBeforeAddEntry);
        builder.AddAttribute(150, "OnAfterAddEntry", OnAfterAddEntry);
        builder.AddAttribute(160, "OnBeforeUpdateEntry", OnBeforeUpdateEntry);
        builder.AddAttribute(170, "OnAfterUpdateEntry", OnAfterUpdateEntry);
        builder.AddAttribute(180, "OnBeforeConvertPropertyType", OnBeforeConvertPropertyType);
        builder.AddAttribute(190, "OnBeforePropertyChanged", OnBeforePropertyChanged);
        builder.AddAttribute(200, "OnAfterPropertyChanged", OnAfterPropertyChanged);
        builder.AddAttribute(210, "OnBeforeSaveChanges", OnBeforeSaveChanges);
        builder.AddAttribute(220, "OnAfterSaveChanges", OnAfterSaveChanges);
        builder.AddAttribute(230, "OnCreateNewListEntryInstance", OnCreateNewListEntryInstance);
        builder.AddAttribute(240, "OnBeforeAddListEntry", OnBeforeAddListEntry);
        builder.AddAttribute(250, "OnAfterAddListEntry", OnAfterAddListEntry);
        builder.AddAttribute(260, "OnBeforeRemoveListEntry", OnBeforeRemoveListEntry);
        builder.AddAttribute(270, "OnAfterRemoveListEntry", OnAfterRemoveListEntry);
        builder.AddAttribute(280, "OnBeforeConvertListPropertyType", OnBeforeConvertListPropertyType);
        builder.AddAttribute(290, "OnBeforeListPropertyChanged", OnBeforeListPropertyChanged);
        builder.AddAttribute(300, "OnAfterListPropertyChanged", OnAfterListPropertyChanged);
        builder.AddAttribute(310, "OnAfterMoveListEntryUp", OnAfterMoveListEntryUp);
        builder.AddAttribute(320, "OnAfterMoveListEntryDown", OnAfterMoveListEntryDown);

        builder.AddComponentReferenceCapture(1000, (card) => BaseCard = (IBaseCard?)card);

        builder.CloseComponent();
    };

    #endregion

    public async Task ShowModalAsync(bool addingMode = false, bool viewMode = false, object?[]? primaryKeys = null, IBaseModel? template = null)
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
            HideModal();
    }

    public void HideModal()
    {
        Modal?.Hide();
    }

    public async Task OnModalClosing(ModalClosingEventArgs args)
    {
        if (!ContinueByUnsavedChanges && await HasUnsavedChangesAsync())
        {
            args.Cancel = true;
            return;
        }

        BaseCard?.ResetCard();
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
            onClosing: (closingArgs, result) => UserHandleUnsavedChangesConfirmDialog(result));

        return true;
    }

    protected virtual Task UserHandleUnsavedChangesConfirmDialog(ConfirmDialogResult result)
    {
        if (result == ConfirmDialogResult.Aborted)
            return Task.CompletedTask;

        ContinueByUnsavedChanges = true;
        HideModal();

        return Task.CompletedTask;
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