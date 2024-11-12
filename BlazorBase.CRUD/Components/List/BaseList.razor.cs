using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
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

    #endregion

    #region Injects

    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Members
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
    }

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

    public virtual Task HideCardModalAsync()
    {
        if (BaseModalCard != null)
            return BaseModalCard.HideModalAsync();

        return Task.CompletedTask;
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
