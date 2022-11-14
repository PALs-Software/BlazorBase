using BlazorBase.CRUD.Enums;
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
using Microsoft.AspNetCore.WebUtilities;
using BlazorBase.MessageHandling.Enum;
using Blazorise;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Card;

namespace BlazorBase.CRUD.Components.List
{
    public partial class BaseList<TModel> : BaseDisplayComponent, IDisposable where TModel : class, IBaseModel, new()
    {
        #region Parameters

        #region Events

        #region Model
        [Parameter] public EventCallback OnCardClosed { get; set; }
        [Parameter] public EventCallback<OnCreateNewEntryInstanceArgs> OnCreateNewEntryInstance { get; set; }
        [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
        [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnBeforeRemoveEntryArgs> OnBeforeRemoveEntry { get; set; }
        [Parameter] public EventCallback<OnAfterRemoveEntryArgs> OnAfterRemoveEntry { get; set; }
        [Parameter] public EventCallback<OnAfterCardSaveChangesArgs> OnAfterSaveChanges { get; set; }
        #endregion

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

        #region BaseList            
        [Parameter] public EventCallback<OnBeforeOpenAddModalArgs> OnBeforeOpenAddModal { get; set; }
        [Parameter] public EventCallback<OnBeforeOpenEditModalArgs> OnBeforeOpenEditModal { get; set; }
        [Parameter] public EventCallback<OnBeforeOpenViewModalArgs> OnBeforeOpenViewModal { get; set; }
        #endregion

        #endregion

        [Parameter] public bool HideTitle { get; set; } = false;
        [Parameter] public string SingleDisplayName { get; set; }
        [Parameter] public string ExplainText { get; set; }
        [Parameter] public string PluralDisplayName { get; set; }
        [Parameter] public List<Expression<Func<IBaseModel, bool>>> DataLoadConditions { get; set; }

        [Parameter] public bool UserCanAddEntries { get; set; } = true;
        [Parameter] public bool UserCanEditEntries { get; set; } = true;
        [Parameter] public bool UserCanOpenCardReadOnly { get; set; } = false;
        [Parameter] public bool UserCanDeleteEntries { get; set; } = true;

        [Parameter] public bool ShowEntryByStart { get; set; }
        [Parameter] public TModel ComponentModelInstance { get; set; }
        [Parameter] public bool DontRenderCard { get; set; }
        [Parameter] public bool Sortable { get; set; } = true;
        [Parameter] public bool Filterable { get; set; } = true;
        [Parameter] public bool UrlNavigationEnabled { get; set; } = true;
        [Parameter] public Dictionary<string, Enums.SortDirection> InitalSortPropertyColumns { get; set; } = new();

        [Parameter] public RenderFragment<TModel> AdditionalRowButtons { get; set; }
        [Parameter] public RenderFragment AdditionalHeaderButtons { get; set; }

        #region Style
        [Parameter] public string TableClass { get; set; }
        #endregion

        #endregion

        #region Injects

        [Inject] public BaseService Service { get; set; }
        [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; }
        [Inject] protected IStringLocalizer<BaseList<TModel>> Localizer { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected BaseParser BaseParser { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }

        #endregion

        #region Members
        protected EventServices EventServices;

        protected List<TModel> Entries = new();
        protected Type TModelType;

        protected BaseModalCard<TModel> BaseModalCard = default!;
        protected Virtualize<TModel> VirtualizeList = default!;

        protected List<IBasePropertyListDisplay> PropertyListDisplays = new();

        protected bool IsSelfNavigating = false;
        protected string ListNavigationBasePath;
        protected EventHandler<LocationChangedEventArgs> LocationEventHandler;
        protected List<DisplayItem> SortedColumns = new();
        #endregion

        #region Init

        #region Component Creation

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                EventServices = GetEventServices(Service);

                TModelType = typeof(TModel);
                SetUpDisplayLists(TModelType, GUIType.List, ComponentModelInstance);

                SetDisplayNames();
                PropertyListDisplays = ServiceProvider.GetServices<IBasePropertyListDisplay>().ToList();

                ListNavigationBasePath = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;
                if (UrlNavigationEnabled)
                {
                    LocationEventHandler = async (sender, args) => await NavigationManager_LocationChanged(sender, args);
                    NavigationManager.LocationChanged += LocationEventHandler;
                }

                SetInitalSortOfPropertyColumns();
            });

            await PrepareForeignKeyProperties(Service);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await ProcessQueryParameters();
        }

        public virtual void Dispose()
        {
            if (UrlNavigationEnabled)
                NavigationManager.LocationChanged -= LocationEventHandler;
        }

        protected virtual async Task NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            if (IsSelfNavigating)
            {
                IsSelfNavigating = false;
                return;
            }

            if (UrlNavigationEnabled)
                await ProcessQueryParameters();
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            SetDisplayNames();
            if (VirtualizeList != null)
                await VirtualizeList.RefreshDataAsync();
            await InvokeAsync(() => StateHasChanged());
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

            if (String.IsNullOrEmpty(ExplainText))
                ExplainText = ModelLocalizer["ExplainText"];

            if (ExplainText == "ExplainText")
                ExplainText = null;
        }

        protected virtual async Task<RenderFragment> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, TModel model)
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

        #region Navigation

        protected virtual async Task ProcessQueryParameters()
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
                if (query.TryGetValue(keyProperty.Name, out var keyValue) && BaseParser.TryParseValueFromString(keyProperty.PropertyType, keyValue.ToString(), out object parsedValue, out string errorMessage))
                    primaryKeys.Add(parsedValue);
                else
                {
                    ChangeUrlToList();
                    return;
                }

            await NavigateToEntryAsync(primaryKeys.ToArray());
        }

        protected virtual async Task NavigateToEntryAsync(params object[] primaryKeys)
        {
            var entry = await Service.GetAsync<TModel>(primaryKeys);

            if (entry == null)
            {
                ChangeUrlToList();
                return;
            }

            if (DataLoadConditions != null)
                foreach (var dataLoadCondition in DataLoadConditions) //Check all data load conditions if user is allowed to see this entry
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

        #region Data Loading
        protected virtual async ValueTask<ItemsProviderResult<TModel>> LoadListDataProviderAsync(ItemsProviderRequest request)
        {
            if (request.Count == 0)
                return new ItemsProviderResult<TModel>(new List<TModel>(), 0);

            var query = CreateLoadDataQuery();

            var totalEntries = await query.CountAsync();
            Entries = await query.Skip(request.StartIndex).Take(request.Count).ToListAsync();

            return new ItemsProviderResult<TModel>(Entries, totalEntries);
        }

        protected virtual IQueryable<TModel> CreateLoadDataQuery()
        {
            var baseService = ServiceProvider.GetService<BaseService>(); //Use own service for each call, because then the queries can run parallel, because this method get called multiple times at the same time

            var query = baseService.Set<TModel>();
            foreach (var sortedColumn in SortedColumns)
                foreach (var displayProperty in sortedColumn.DisplayPropertyPath.Split("|"))
                {
                    if (sortedColumn.SortDirection == Enums.SortDirection.Ascending)
                        query = query is IOrderedQueryable<TModel> orderedQuery ? orderedQuery.ThenBy(displayProperty) : query.OrderBy(displayProperty);
                    else
                        query = query is IOrderedQueryable<TModel> orderedQuery ? orderedQuery.ThenByDescending(displayProperty) : query.OrderByDescending(displayProperty);
                }

            if (DataLoadConditions != null)
                foreach (var dataLoadCondition in DataLoadConditions)
                    if (dataLoadCondition != null)
                        query = query.Where(dataLoadCondition).Cast<TModel>();

            foreach (var group in DisplayGroups)
                foreach (var displayItem in group.Value.DisplayItems)
                    query = query.Where(displayItem);

            return query;
        }

        #endregion

        #region Display
        protected virtual string DisplayForeignKey(DisplayItem displayItem, TModel model)
        {
            var key = displayItem.Property.GetValue(model)?.ToString();
            var foreignKeyPair = ForeignKeyProperties[displayItem.Property].FirstOrDefault(entry => entry.Key == key);

            if (foreignKeyPair.Equals(default(KeyValuePair<string, string>)))
                return key;
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

            await VirtualizeList.RefreshDataAsync();
        }
        #endregion

        #region Filtering
        protected virtual async Task OnFilterChangedAsync()
        {
            await VirtualizeList.RefreshDataAsync();
        }
        #endregion

        #region CRUD

        public virtual async Task AddEntryAsync()
        {
            var args = new OnBeforeOpenAddModalArgs(false, EventServices);
            await OnBeforeOpenAddModal.InvokeAsync(args);
            if (args.IsHandled)
                return;

            await BaseModalCard.ShowModalAsync(addingMode: true);
        }

        public virtual async Task EditEntryAsync(TModel entry, bool changeQueryUrl = true)
        {
            var args = new OnBeforeOpenEditModalArgs(false, entry, changeQueryUrl, EventServices);
            await OnBeforeOpenEditModal.InvokeAsync(args);

            if (args.ChangeQueryUrl)
                ChangeUrlToEntry(entry);

            if (args.IsHandled)
                return;

            await BaseModalCard.ShowModalAsync(addingMode: false, viewMode: false, entry.GetPrimaryKeys());
        }

        public virtual async Task ViewEntryAsync(TModel entry, bool changeQueryUrl = true)
        {
            var args = new OnBeforeOpenViewModalArgs(false, entry, changeQueryUrl, EventServices);
            await OnBeforeOpenViewModal.InvokeAsync(args);

            if (args.ChangeQueryUrl)
                ChangeUrlToEntry(entry);

            if (args.IsHandled)
                return;

            await BaseModalCard.ShowModalAsync(addingMode: false, viewMode: true, entry.GetPrimaryKeys());
        }

        public virtual async Task RemoveEntryAsync(TModel model)
        {
            if (model == null)
                return;

            await InvokeAsync(() =>
            {
                MessageHandler.ShowConfirmDialog(Localizer["Delete {0}", SingleDisplayName],
                                                    Localizer["Do you really want to delete the entry {0}?", model.GetDisplayKey()],
                                                    confirmButtonText: Localizer["Delete"],
                                                    confirmButtonColor: Color.Danger,
                                                    onClosing: async (args, result) => await OnConfirmDialogClosedAsync(result, model));
            });
        }

        protected virtual async Task OnConfirmDialogClosedAsync(ConfirmDialogResult result, TModel model)
        {
            if (result == ConfirmDialogResult.Aborted)
                return;

            var baseService = ServiceProvider.GetService<BaseService>();
            var scopedModel = await baseService.GetAsync<TModel>(model.GetPrimaryKeys());

            var eventServices = GetEventServices(baseService);

            var beforeRemoveArgs = new OnBeforeRemoveEntryArgs(scopedModel, false, eventServices);
            await OnBeforeRemoveEntry.InvokeAsync(beforeRemoveArgs);
            await scopedModel.OnBeforeRemoveEntry(beforeRemoveArgs);
            if (beforeRemoveArgs.AbortRemoving)
                return;

            try
            {
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

            await VirtualizeList.RefreshDataAsync();
            await InvokeAsync(() => StateHasChanged());
        }

        protected virtual async Task OnCardClosedAsync()
        {
            await VirtualizeList.RefreshDataAsync();
            ChangeUrlToList();

            await OnCardClosed.InvokeAsync();
        }
        #endregion


        #region Actions
        public virtual async Task RefreshDataAsync()
        {
            await VirtualizeList.RefreshDataAsync();
        }
        #endregion

        #region Other
        protected virtual EventServices GetEventServices(BaseService baseService)
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = baseService,
                MessageHandler = MessageHandler
            };
        }
        #endregion
    }
}
