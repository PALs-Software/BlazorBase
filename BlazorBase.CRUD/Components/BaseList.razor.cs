using BlazorBase.Components;
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static BlazorBase.CRUD.Models.IBaseModel;
using Microsoft.AspNetCore.WebUtilities;
using BlazorBase.MessageHandling.Enum;
using Blazorise;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseList<TModel> : BaseDisplayComponent where TModel : class, IBaseModel, new()
    {
        #region Parameters

        #region Events
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
        [Parameter] public EventCallback<OnAfterSaveChangesArgs> OnAfterSaveChanges { get; set; }

        #region List Events
        [Parameter] public EventCallback<OnCreateNewListEntryInstanceArgs> OnCreateNewListEntryInstance { get; set; }
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateListEntryArgs> OnBeforeUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateListEntryArgs> OnAfterUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeConvertListPropertyTypeArgs> OnBeforeConvertListPropertyType { get; set; }
        [Parameter] public EventCallback<OnBeforeListPropertyChangedArgs> OnBeforeListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterListPropertyChangedArgs> OnAfterListPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnBeforeRemoveListEntryArgs> OnBeforeRemoveListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterRemoveListEntryArgs> OnAfterRemoveListEntry { get; set; }
        #endregion
        #endregion

        [Parameter]
        public string SingleDisplayName { get; set; }

        [Parameter]
        public string PluralDisplayName { get; set; }

        [Parameter]
        public Func<TModel, bool> DataLoadCondition { get; set; }


        #endregion

        #region Injects

        [Inject] public BaseService Service { get; set; }
        [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; }
        [Inject]
        protected StringLocalizerFactory GenericClassStringLocalizer { get; set; }
        protected IStringLocalizer Localizer { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected BaseParser BaseParser { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }

        #endregion

        #region Members
        protected List<TModel> Entries = new List<TModel>();
        protected Type TModelType;

        protected BaseCard<TModel> BaseCard = default!;
        protected Virtualize<TModel> VirtualizeList = default!;

        protected List<IBasePropertyListDisplay> PropertyListDisplays = new List<IBasePropertyListDisplay>();
        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Localizer = GenericClassStringLocalizer.GetLocalizer(typeof(BaseList<TModel>));
                TModelType = typeof(TModel);

                SetUpDisplayLists(TModelType, GUIType.List);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[TModelType.Name];
                if (String.IsNullOrEmpty(PluralDisplayName))
                    PluralDisplayName = ModelLocalizer[$"{TModelType.Name}_Plural"];

                PropertyListDisplays = ServiceProvider.GetServices<IBasePropertyListDisplay>().ToList();

            });

            await ProcessQueryParameters();
        }

        protected async Task<RenderFragment> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, TModel model)
        {
            var eventServices = GetEventServices();

            foreach (var propertyListDisplay in PropertyListDisplays)
                if (await propertyListDisplay.IsHandlingPropertyRenderingAsync(model, displayItem, eventServices))
                    return GetPropertyListDisplayExtensionAsRenderFragment(displayItem, propertyListDisplay.GetType(), model);

            return null;
        }

        protected RenderFragment GetPropertyListDisplayExtensionAsRenderFragment(DisplayItem displayItem, Type baseInputExtensionType, TModel model) => builder =>
        {
            builder.OpenComponent(0, baseInputExtensionType);

            builder.AddAttribute(1, "Model", model);
            builder.AddAttribute(2, "Property", displayItem.Property);
            builder.AddAttribute(3, "Service", Service);
            builder.AddAttribute(4, "ModelLocalizer", ModelLocalizer);

            builder.CloseComponent();
        };

        private async ValueTask<ItemsProviderResult<TModel>> LoadListDataProviderAsync(ItemsProviderRequest request)
        {
            var baseService = ServiceProvider.GetService<BaseService>(); //Use own service for each call, because then the queries can run parallel, because this method get called multiple times at the same time
            
            int totalEntries;
            if (DataLoadCondition == null)
            {
                Entries = await baseService.GetDataAsync<TModel>(request.StartIndex, request.Count);
                totalEntries = await baseService.CountDataAsync<TModel>();
            }
            else
            {
                Entries = await baseService.GetDataAsync(DataLoadCondition, request.StartIndex, request.Count);
                totalEntries = await baseService.CountDataAsync(DataLoadCondition);
            }

            return new ItemsProviderResult<TModel>(Entries, totalEntries);
        }

        private async Task ProcessQueryParameters()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);
            if (query.Count == 0)
                return;

            var keyProperties = TModelType.GetProperties().Where(property =>
               (!property.PropertyType.IsSubclassOf(typeof(IBaseModel))) &&
               property.IsKey()
           ).ToList();

            var primaryKeys = new List<object>();
            foreach (var keyProperty in keyProperties)
                if (query.TryGetValue(keyProperty.Name, out var keyValue))
                    if (BaseParser.TryParseValueFromString(keyProperty.PropertyType, keyValue.ToString(), out object parsedValue, out string errorMessage))
                        primaryKeys.Add(parsedValue);
                    else
                        return;

            var entry = await Service.GetAsync<TModel>(primaryKeys.ToArray());
            if (entry != null)
                await EditEntryAsync(entry, false);
        }

        protected void NavigateToEntry(TModel entry)
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = entry.GetNavigationQuery(uri.Query);

            var newUrl = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), query);
            NavigationManager.NavigateTo(newUrl);
        }

        protected void NavigateToList()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = RemoveNavigationQueryByType(TModelType, uri.Query);

            var newUrl = QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), query);
            NavigationManager.NavigateTo(newUrl);
        }
        #endregion

        #region CRUD

        protected async Task AddEntryAsync()
        {
            await BaseCard.ShowAsync(addingMode: true);
        }
        protected async Task EditEntryAsync(TModel entry, bool changeQueryUrl = true)
        {
            if (changeQueryUrl)
                NavigateToEntry(entry);

            await BaseCard.ShowAsync(addingMode: false, entry.GetPrimaryKeys());
        }

        protected async Task RemoveEntryAsync(TModel model)
        {
            if (model == null)
                return;

            await InvokeAsync(() =>
            {
                var primaryKeyString = String.Join(", ", model.GetPrimaryKeys());

                MessageHandler.ShowConfirmDialog(Localizer["Delete {0}", SingleDisplayName],
                                                    Localizer["Do you really want to delete the entry {0}?", primaryKeyString],
                                                    confirmButtonText: Localizer["Delete"],
                                                    confirmButtonColor: Color.Danger,
                                                    onClosing: async (args, result) => await OnConfirmDialogClosedAsync(result, model));
            });
        }

        protected async Task OnConfirmDialogClosedAsync(ConfirmDialogResult result, TModel model)
        {
            if (result == ConfirmDialogResult.Aborted)
                return;

            var eventServices = GetEventServices();

            var beforeRemoveArgs = new OnBeforeRemoveEntryArgs(model, false, eventServices);
            await OnBeforeRemoveEntry.InvokeAsync(beforeRemoveArgs);
            await model.OnBeforeRemoveEntry(beforeRemoveArgs);
            if (beforeRemoveArgs.AbortRemoving)
                return;

            try
            {
                await Service.RemoveEntryAsync(model);
                await Service.SaveChangesAsync();
                Entries.Remove(model);

                var afterRemoveArgs = new OnAfterRemoveEntryArgs(model, eventServices);
                await OnAfterRemoveEntry.InvokeAsync(afterRemoveArgs);
                await model.OnAfterRemoveEntry(afterRemoveArgs);
            }
            catch (Exception e)
            {
                MessageHandler.ShowMessage(Localizer["Error while deleting"], PrepareExceptionErrorMessage(e), MessageType.Error);
            }

            await VirtualizeList.RefreshDataAsync();
            await InvokeAsync(() => StateHasChanged());
        }

        protected async Task OnCardClosedAsync()
        {
            await VirtualizeList.RefreshDataAsync();
            NavigateToList();

            await OnCardClosed.InvokeAsync();
        }
        #endregion

        #region Other
        protected EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = Service,
                MessageHandler = MessageHandler
            };
        }
        #endregion
    }
}
