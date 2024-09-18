using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Extensions;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.CRUD.Components.PageActions;
using BlazorBase.CRUD.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.List;

public partial class BaseList<TModel> : BaseGenericList<TModel>, IDisposable where TModel : class, IBaseModel, new()
{
    #region Parameters

    [Parameter] public bool UserCanAddEntries { get; set; } = true;
    [Parameter] public bool UserCanEditEntries { get; set; } = true;
    [Parameter] public bool UserCanOpenCardReadOnly { get; set; } = false;

    [Parameter] public bool ShowEntryByStart { get; set; }
    [Parameter] public bool DontRenderCard { get; set; }
    [Parameter] public bool UrlNavigationEnabled { get; set; } = true;

    #region Events

    #region Model
    [Parameter] public EventCallback OnCardClosed { get; set; }
    [Parameter] public EventCallback<OnCreateNewEntryInstanceArgs> OnCreateNewEntryInstance { get; set; }
    [Parameter] public EventCallback<OnGuiLoadDataArgs> OnGuiLoadData { get; set; }
    [Parameter] public EventCallback<OnShowEntryArgs> OnShowEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
    [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
    [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterCardSaveChangesArgs> OnAfterSaveChanges { get; set; }
    [Parameter] public EventCallback<OnBeforeCardSaveChangesArgs> OnBeforeSaveChanges { get; set; }
    #endregion

    #region List Part Events
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

    #region BaseList            
    [Parameter] public EventCallback<OnBeforeOpenAddModalArgs> OnBeforeOpenAddModal { get; set; }
    [Parameter] public EventCallback<OnBeforeOpenEditModalArgs> OnBeforeOpenEditModal { get; set; }
    [Parameter] public EventCallback<OnBeforeOpenViewModalArgs> OnBeforeOpenViewModal { get; set; }

    [Parameter] public EventCallback<OnBeforeNavigateToEntryArgs> OnBeforeNavigateToEntry { get; set; }
    #endregion

    #endregion

    #region Custom Render
    [Parameter] public Type? CustomBaseModalCardType { get; set; } = null;
    [Parameter] public Type? CustomBaseCardType { get; set; } = null;
    #endregion

    #endregion

    #region Injects

    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Members
    protected IBaseModalCard? BaseModalCard = null!;
    protected RenderFragment? BaseModalCardRenderFragment;
    protected BasePageActions? PageActions = null;
    protected bool IsSelfNavigating = false;
    protected string ListNavigationBasePath = null!;
    protected EventHandler<LocationChangedEventArgs>? LocationEventHandler;

    protected bool CardIsCurrentlyInAddingMode = false;
    #endregion

    #region Init

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        ListNavigationBasePath = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;
        if (UrlNavigationEnabled)
        {
            LocationEventHandler = async (sender, args) => await NavigationManager_LocationChanged(sender, args);
            NavigationManager.LocationChanged += LocationEventHandler;
        }

        if (CustomBaseModalCardType == null)
            BaseModalCardRenderFragment = CreateBaseModalCardRenderFragment(typeof(BaseModalCard<TModel>));
        else
            BaseModalCardRenderFragment = CreateBaseModalCardRenderFragment(CustomBaseModalCardType);
    }

    protected virtual RenderFragment CreateBaseModalCardRenderFragment(Type type) => builder =>
    {
        builder.OpenComponent(0, type);
        
        builder.AddAttribute(1, "SingleDisplayName", SingleDisplayName);
        builder.AddAttribute(2, "ExplainText", ExplainText);
        builder.AddAttribute(3, "ComponentModelInstance", ComponentModelInstance);
        builder.AddAttribute(4, "OnCardClosed", EventCallback.Factory.Create(this, OnCardClosedAsync));

        builder.AddAttribute(5, "OnShowEntry", OnShowEntry);
        builder.AddAttribute(6, "AdditionalHeaderPageActions", AdditionalHeaderPageActions);

        builder.AddAttribute(50, "CustomBaseCardType", CustomBaseCardType);

        builder.AddAttribute(100, "OnAfterGetVisibleProperties", OnAfterGetVisibleProperties);
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
        builder.AddAttribute(210, "OnBeforeSaveChanges", EventCallback.Factory.Create<OnBeforeCardSaveChangesArgs>(this, OnBeforeSaveChangesAsync));
        builder.AddAttribute(220, "OnAfterSaveChanges", EventCallback.Factory.Create<OnAfterCardSaveChangesArgs>(this, OnAfterSaveChangesAsync));
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

        builder.AddComponentReferenceCapture(1000, (modalCard) => BaseModalCard = (IBaseModalCard?)modalCard);

        builder.CloseComponent();
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await ProcessQueryParameters(isFirstPageLoadNavigation: true);
    }

    public virtual void Dispose()
    {
        if (UrlNavigationEnabled && LocationEventHandler != null)
            NavigationManager.LocationChanged -= LocationEventHandler;
    }

    protected virtual async Task NavigationManager_LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (IsSelfNavigating)
        {
            IsSelfNavigating = false;
            return;
        }

        if (UrlNavigationEnabled)
            await ProcessQueryParameters();
    }

    #region Navigation

    protected virtual async Task ProcessQueryParameters(bool isFirstPageLoadNavigation = false)
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

        if (uri.AbsolutePath != ListNavigationBasePath)
            return;

        if (!UserCanEditEntries && !UserCanOpenCardReadOnly)
            return;

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.Count == 0)
            return;

        var primaryKeys = new List<object>();
        foreach (var keyProperty in TModelType.GetKeyProperties())
            if (query.TryGetValue(keyProperty.Name, out var keyValue) && BaseParser.TryParseValueFromString(keyProperty.PropertyType, keyValue.ToString(), out object? parsedValue, out string? _))
            {
                if (parsedValue != null)
                    primaryKeys.Add(parsedValue);
            }
            else
            {
                ChangeUrlToList();
                return;
            }

        await NavigateToEntryAsync(isFirstPageLoadNavigation, primaryKeys.ToArray());
    }

    protected virtual async Task NavigateToEntryAsync(bool isFirstPageLoadNavigation, params object[] primaryKeys)
    {
        var entry = await DbContext.FindAsync<TModel>(primaryKeys);
        var args = new OnBeforeNavigateToEntryArgs(entry, isFirstPageLoadNavigation, EventServices);
        await OnBeforeNavigateToEntry.InvokeAsync(args);

        if (args.IsHandled)
            return;

        if (entry == null)
        {
            ChangeUrlToList();
            return;
        }

        var dataLoadConditions = DataLoadConditions;
        if (args.UseCustomDataLoadConditionsForCheck)
            dataLoadConditions = args.CustomDataLoadConditions;

        if (dataLoadConditions != null)
            foreach (var dataLoadCondition in dataLoadConditions) //Check all data load conditions if user is allowed to see this entry
                if (dataLoadCondition != null && !dataLoadCondition.Compile()(entry))
                {
                    ChangeUrlToList();
                    return;
                }

        if (UserCanOpenCardReadOnly)
            await ViewEntryAsync(entry, false);
        else if (UserCanEditEntries)
            await EditEntryAsync(entry, false);

    }

    protected virtual void ChangeUrlToEntry(TModel entry)
    {
        IsSelfNavigating = true;
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = entry.GetNavigationQuery(uri.Query);

        var newUrl = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), query);
        NavigationManager.NavigateTo(newUrl);
    }

    protected virtual void ChangeUrlToList()
    {
        IsSelfNavigating = true;
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = RemoveNavigationQueryByType(TModelType, uri.Query);

        var newUrl = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), query);
        NavigationManager.NavigateTo(newUrl);
    }
    #endregion

    #endregion

    #region CRUD

    public virtual Task OnRowDoubleClicked(TModel entry)
    {
        if (UserCanEditEntries)
            return EditEntryAsync(entry);
        else if (UserCanOpenCardReadOnly)
            return ViewEntryAsync(entry);

        return Task.CompletedTask;
    }

    public virtual async Task AddEntryAsync(TModel? template = null)
    {
        var args = new OnBeforeOpenAddModalArgs(false, EventServices);
        await OnBeforeOpenAddModal.InvokeAsync(args);
        if (args.IsHandled)
            return;

        if (BaseModalCard != null)
        {
            CardIsCurrentlyInAddingMode = true;
            await BaseModalCard.ShowModalAsync(addingMode: true, template: template);
        }   
    }

    public virtual async Task EditEntryAsync(TModel entry, bool changeQueryUrl = true)
    {
        var args = new OnBeforeOpenEditModalArgs(false, entry, changeQueryUrl, EventServices);
        await OnBeforeOpenEditModal.InvokeAsync(args);

        if (args.ChangeQueryUrl)
            ChangeUrlToEntry(entry);

        if (args.IsHandled)
            return;

        if (BaseModalCard != null)
        {
            CardIsCurrentlyInAddingMode = false;
            await BaseModalCard.ShowModalAsync(addingMode: false, viewMode: false, entry.GetPrimaryKeys());
        }   
    }

    public virtual async Task ViewEntryAsync(TModel entry, bool changeQueryUrl = true)
    {
        var args = new OnBeforeOpenViewModalArgs(false, entry, changeQueryUrl, EventServices);
        await OnBeforeOpenViewModal.InvokeAsync(args);

        if (args.ChangeQueryUrl)
            ChangeUrlToEntry(entry);

        if (args.IsHandled)
            return;

        if (BaseModalCard != null)
        {
            CardIsCurrentlyInAddingMode = false;
            await BaseModalCard.ShowModalAsync(addingMode: false, viewMode: true, entry.GetPrimaryKeys());
        }   
    }

    public virtual void HideCardModal()
    {
        if (BaseModalCard != null)
            BaseModalCard.HideModal();
    }


    #endregion

    #region Events

    protected virtual async Task OnCardClosedAsync()
    {
        await RefreshDataAsync();
        ChangeUrlToList();

        await OnCardClosed.InvokeAsync();
    }

    public virtual Task OnBeforeSaveChangesAsync(OnBeforeCardSaveChangesArgs args)
    {
        CardIsCurrentlyInAddingMode = BaseModalCard?.CardIsInAddingMode() ?? false;
        return OnBeforeSaveChanges.InvokeAsync(args);
    }

    public virtual Task OnAfterSaveChangesAsync(OnAfterCardSaveChangesArgs args)
    {
        var isInAddingMode = BaseModalCard?.CardIsInAddingMode() ?? false;
        if (CardIsCurrentlyInAddingMode && !isInAddingMode && args.Model is TModel model)
            ChangeUrlToEntry(model);

        CardIsCurrentlyInAddingMode = isInAddingMode;
        return OnAfterSaveChanges.InvokeAsync(args);
    }
    #endregion
}
