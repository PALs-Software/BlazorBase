﻿using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.MessageHandling.Enum;
using Blazorise;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.Models;
using Newtonsoft.Json;
using BlazorBase.CRUD.Components.PageActions.Models;
using BlazorBase.CRUD.Attributes;
using System.Reflection;

namespace BlazorBase.CRUD.Components.List;

public partial class BaseGenericList<TModel> : BaseDisplayComponent where TModel : class, IBaseModel, new()
{
    #region Parameters

    [Parameter] public bool UserCanDeleteEntries { get; set; } = true;

    [Parameter] public bool HideTitle { get; set; } = false;
    [Parameter] public string? SingleDisplayName { get; set; }
    [Parameter] public ExplainText? ExplainText { get; set; }
    [Parameter] public string? PluralDisplayName { get; set; }
    [Parameter] public List<Expression<Func<IBaseModel, bool>>>? DataLoadConditions { get; set; }

    [Parameter] public TModel ComponentModelInstance { get; set; } = null!;
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public bool Filterable { get; set; } = true;
    [Parameter] public bool StickyRowButtons { get; set; } = true;
    [Parameter] public bool HideRowButtons { get; set; } = false;
    [Parameter] public Dictionary<string, Enums.SortDirection> InitalSortPropertyColumns { get; set; } = new();


    [Parameter] public RenderFragment<TModel>? AdditionalRowButtons { get; set; }
    [Parameter] public RenderFragment<AdditionalHeaderPageActionsArgs> AdditionalHeaderPageActions { get; set; } = null!;

    #region Style
    [Parameter] public string? TableClass { get; set; }
    #endregion

    #region Events

    #region Models
    [Parameter] public EventCallback<OnBeforeRemoveEntryArgs> OnBeforeRemoveEntry { get; set; }
    [Parameter] public EventCallback<OnAfterRemoveEntryArgs> OnAfterRemoveEntry { get; set; }
    #endregion

    #region BaseList            
    [Parameter] public EventCallback<OnAfterEntrySelectedArgs> OnAfterEntrySelected { get; set; }
    #endregion

    #endregion

    #endregion

    #region Injects

    [Inject] public BaseService Service { get; set; } = null!;
    [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<BaseList<TModel>> Localizer { get; set; } = null!;
    [Inject] protected BaseParser BaseParser { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IBlazorBaseOptions BlazorBaseOptions { get; set; } = null!;

    #endregion

    #region Members
    protected EventServices EventServices = null!;

    protected List<TModel> Entries = new();
    protected object?[]? SelectedEntryPrimaryKeys = null;
    protected Type TModelType = null!;

    protected BaseModalCard<TModel>? BaseModalCard = null!;
    protected Virtualize<TModel>? VirtualizeList = null!;

    protected List<IBasePropertyListDisplay> PropertyListDisplays = new();

    protected List<DisplayItem> SortedColumns = new();
    #endregion

    #region Init

    #region Component Creation

    protected override async Task OnInitializedAsync()
    {
        if (ComponentModelInstance == null)
            ComponentModelInstance = new TModel();

        EventServices = GetEventServices(Service);

        TModelType = typeof(TModel);
        await SetUpDisplayListsAsync(TModelType, GUIType.List, ComponentModelInstance);

        SetDisplayNames();
        PropertyListDisplays = ServiceProvider.GetServices<IBasePropertyListDisplay>().ToList();

        SetInitalSortOfPropertyColumns();

        await PrepareForeignKeyProperties(Service);
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        SetDisplayNames();
        if (VirtualizeList != null)
            await VirtualizeList.RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected virtual void SetDisplayNames()
    {
        if (String.IsNullOrEmpty(SingleDisplayName))
            SingleDisplayName = ModelLocalizer[TModelType.Name];
        else
            SingleDisplayName = ModelLocalizer[SingleDisplayName];

        if (String.IsNullOrEmpty(PluralDisplayName))
            PluralDisplayName = ModelLocalizer[$"{TModelType.Name}_Plural"];
        else
            PluralDisplayName = ModelLocalizer[PluralDisplayName];

        if (ExplainText == null && ModelLocalizer["ExplainText"] != "ExplainText")
            ExplainText = new ExplainText(ModelLocalizer["ExplainText"], ModelLocalizer["ExplainText_Location"] == "Bottom" ? ExplainTextLocation.Bottom : ExplainTextLocation.Top);
    }

    protected virtual async Task<RenderFragment?> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, TModel model)
    {
        foreach (var propertyListDisplay in PropertyListDisplays)
            if (await propertyListDisplay.IsHandlingPropertyRenderingAsync(model, displayItem, EventServices))
                return GetPropertyListDisplayExtensionAsRenderFragment(displayItem, propertyListDisplay.GetType(), model);

        return null;
    }

    protected virtual RenderFragment GetPropertyListDisplayExtensionAsRenderFragment(DisplayItem displayItem, Type baseInputExtensionType, TModel model) => builder =>
    {
        builder.OpenComponent(0, baseInputExtensionType);

        builder.AddAttribute(1, "Model", model);
        builder.AddAttribute(2, "Property", displayItem.Property);
        builder.AddAttribute(3, "Service", Service);
        builder.AddAttribute(4, "ModelLocalizer", ModelLocalizer);
        builder.AddAttribute(5, "DisplayItem", displayItem);

        builder.CloseComponent();
    };

    protected virtual void SetInitalSortOfPropertyColumns()
    {
        if (!Sortable)
            return;

        foreach (var group in DisplayGroups)
            foreach (var displayItem in group.Value.DisplayItems.OrderBy(entry => entry.Attribute.SortOrder))
            {
                if (!displayItem.IsSortable)
                    continue;

                var sortedColumn = InitalSortPropertyColumns.Where(entry => entry.Key == displayItem.Property.Name);
                if (sortedColumn.Any())
                    displayItem.SortDirection = sortedColumn.First().Value;

                if (displayItem.SortDirection != Enums.SortDirection.None)
                    SortedColumns.Add(displayItem);
            }
    }

    #endregion

    #endregion

    #region Data Loading
    protected virtual async ValueTask<ItemsProviderResult<TModel>> LoadListDataProviderAsync(ItemsProviderRequest request)
    {
        if (request.Count == 0)
            return new ItemsProviderResult<TModel>(new List<TModel>(), 0);

        var baseService = ServiceProvider.GetRequiredService<BaseService>(); //Use own service for each call, because then the queries can run parallel, because this method get called multiple times at the same time
        var query = CreateLoadDataQuery(baseService.Set<TModel>());

        var totalEntries = await query.CountAsync();
        Entries = await query.Skip(request.StartIndex).Take(request.Count).ToListAsync();

        return new ItemsProviderResult<TModel>(Entries, totalEntries);
    }

    protected virtual IQueryable<TModel> CreateLoadDataQuery(IQueryable<TModel> query, bool useEFFilters = true)
    {
        var hasAlreadyOrderByQuery = false;
        foreach (var sortedColumn in SortedColumns)
            if (sortedColumn.DisplayPropertyPath != null)
                foreach (var displayProperty in sortedColumn.DisplayPropertyPath.Split("|"))
                {
                    if (sortedColumn.SortDirection == Enums.SortDirection.Ascending)
                        query = hasAlreadyOrderByQuery && query is IOrderedQueryable<TModel> orderedQuery ? orderedQuery.ThenBy(displayProperty) : query.OrderBy(displayProperty);
                    else
                        query = hasAlreadyOrderByQuery && query is IOrderedQueryable<TModel> orderedQuery ? orderedQuery.ThenByDescending(displayProperty) : query.OrderByDescending(displayProperty);
                    hasAlreadyOrderByQuery = true;
                }

        if (DataLoadConditions != null)
            foreach (var dataLoadCondition in DataLoadConditions)
                if (dataLoadCondition != null)
                    query = query.Where(dataLoadCondition).Cast<TModel>();

        foreach (var group in DisplayGroups)
            foreach (var displayItem in group.Value.DisplayItems)
                query = query.Where(displayItem, useEFFilters);

        if (ComponentModelInstance != null)
        {
            var args = new OnGuiLoadDataArgs(GUIType.List, ComponentModelInstance, query, EventServices);
            ComponentModelInstance.OnGuiLoadData(args);
            if (args.ListLoadQuery != null)
                query = args.ListLoadQuery.Cast<TModel>();
        }

        if (ComponentModelInstance != null)
        {
            var args = new OnGuiLoadDataArgs(GUIType.List, ComponentModelInstance, query, EventServices);
            ComponentModelInstance.OnGuiLoadData(args);
            if (args.ListLoadQuery != null)
                query = args.ListLoadQuery.Cast<TModel>();
        }

        return query;
    }

    #endregion

    #region Display
    protected virtual string? DisplayForeignKey(DisplayItem displayItem, TModel model)
    {
        var key = displayItem.Property.GetValue(model);
        var primaryKeyAsJson = JsonConvert.SerializeObject(new object?[] { key });

        var foreignKeyPair = ForeignKeyProperties[displayItem.Property].FirstOrDefault(entry => entry.Key == primaryKeyAsJson);

        if (foreignKeyPair.Equals(default(KeyValuePair<string, string>)))
            return key?.ToString();
        else
            return foreignKeyPair.Value;
    }

    protected virtual string DisplayEnum(DisplayItem displayItem, TModel model)
    {
        var value = displayItem.Property.GetValue(model)?.ToString();
        if (value == null)
            return String.Empty;

        var localizer = StringLocalizerFactory.Create(displayItem.DisplayPropertyType);
        return localizer[value];
    }
    #endregion

    #region Sorting
    protected async Task OnSortClicked(DisplayItem displayItem, bool fromRightClicked)
    {
        if (!Sortable || !displayItem.IsSortable)
            return;

        if (fromRightClicked)
        {
            displayItem.SortDirection = displayItem.SortDirection.GetNextSortDirection();

            switch (displayItem.SortDirection)
            {
                case Enums.SortDirection.None:
                    SortedColumns.Remove(displayItem);
                    break;
                case Enums.SortDirection.Ascending:
                    SortedColumns.Add(displayItem);
                    break;
            }
        }
        else
        {
            foreach (var displayGroup in DisplayGroups)
                foreach (var item in displayGroup.Value.DisplayItems)
                    if (displayItem != item)
                        item.SortDirection = Enums.SortDirection.None;

            displayItem.SortDirection = displayItem.SortDirection.GetNextSortDirection();
            SortedColumns.Clear();

            if (displayItem.SortDirection != Enums.SortDirection.None)
                SortedColumns.Add(displayItem);
        }

        if (VirtualizeList != null)
            await VirtualizeList.RefreshDataAsync();
    }
    #endregion

    #region Filtering
    protected virtual async Task OnFilterChangedAsync()
    {
        if (VirtualizeList != null)
            await VirtualizeList.RefreshDataAsync();
    }
    #endregion

    #region CRUD

    public virtual Task OnRowSelected(TModel entry)
    {
        if (entry.PrimaryKeysAreEqual(SelectedEntryPrimaryKeys, useCache: true))
            SelectedEntryPrimaryKeys = null;
        else
            SelectedEntryPrimaryKeys = entry.GetPrimaryKeys(useCache: true);

        return OnAfterEntrySelected.InvokeAsync(new OnAfterEntrySelectedArgs(SelectedEntryPrimaryKeys == null ? null : entry, EventServices));
    }

    public virtual async Task RemoveEntryAsync(TModel model)
    {
        if (model == null)
            return;

        await InvokeAsync(() =>
        {
            MessageHandler.ShowConfirmDialog(Localizer["Delete {0}", SingleDisplayName ?? String.Empty],
                                                Localizer["Do you really want to delete the entry {0}?", model.GetDisplayKey()],
                                                confirmButtonText: Localizer["Delete"],
                                                confirmButtonColor: Color.Danger,
                                                onClosing: async (args, result) => await OnRemoveEntryConfirmDialogClosedAsync(result, model));
        });
    }

    protected virtual async Task OnRemoveEntryConfirmDialogClosedAsync(ConfirmDialogResult result, TModel model)
    {
        if (result == ConfirmDialogResult.Aborted)
            return;

        var baseService = ServiceProvider.GetRequiredService<BaseService>();
        var scopedModel = await baseService.GetAsync<TModel>(model.GetPrimaryKeys());

        if (scopedModel == null)
            return;

        var eventServices = GetEventServices(baseService);

        try
        {
            var beforeRemoveArgs = new OnBeforeRemoveEntryArgs(scopedModel, false, eventServices);
            await OnBeforeRemoveEntry.InvokeAsync(beforeRemoveArgs);
            await scopedModel.OnBeforeRemoveEntry(beforeRemoveArgs);
            if (beforeRemoveArgs.AbortRemoving)
                return;

            await baseService.RemoveEntryAsync(scopedModel);
            await baseService.SaveChangesAsync();
            Entries.Remove(scopedModel);

            var afterRemoveArgs = new OnAfterRemoveEntryArgs(scopedModel, eventServices);
            await OnAfterRemoveEntry.InvokeAsync(afterRemoveArgs);
            await scopedModel.OnAfterRemoveEntry(afterRemoveArgs);
        }
        catch (Exception e)
        {
            MessageHandler.ShowMessage(Localizer["Error while deleting"], ErrorHandler.PrepareExceptionErrorMessage(e), MessageType.Error);
        }

        if (VirtualizeList != null)
            await VirtualizeList.RefreshDataAsync();

        await InvokeAsync(StateHasChanged);
    }

    #endregion


    #region Actions
    public virtual async Task RefreshDataAsync()
    {
        if (VirtualizeList != null)
            await VirtualizeList.RefreshDataAsync();
    }
    #endregion

    #region Other
    protected virtual EventServices GetEventServices(BaseService baseService)
    {
        return new EventServices(ServiceProvider, ModelLocalizer, baseService);
    }
    #endregion
}