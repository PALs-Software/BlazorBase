using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseCard<TModel> : BaseDisplayComponent where TModel : class, IBaseModel, new()
    {
        #region Parameter

        #region Events
        [Parameter] public EventCallback<OnCreateNewEntryInstanceArgs> OnCreateNewEntryInstance { get; set; }
        [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
        [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterCardSaveChangesArgs> OnAfterSaveChanges { get; set; }

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

        [Parameter] public string SingleDisplayName { get; set; }
        [Parameter] public bool Embedded { get; set; }
        [Parameter] public bool ShowEntryByStart { get; set; }
        [Parameter] public Func<EventServices, Task<IBaseModel>> EntryToBeShownByStart { get; set; }
        [Parameter] public TModel ComponentModelInstance { get; set; }
        [Parameter] public bool ShowActions { get; set; } = true;
        #endregion

        #region Injects
        [Inject] protected BaseService Service { get; set; }
        [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; }
        [Inject] protected IStringLocalizer<BaseCard<TModel>> Localizer { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        #endregion

        #region Member
        protected EventServices EventServices;

        protected Snackbar Snackbar;      

        protected TModel Model = null;
        protected Type TModelType;

        protected bool ModelLoaded = false;
        protected bool AddingMode;

        protected ValidationContext ValidationContext;
        #endregion

        #region Property Infos
        protected List<BaseInput> BaseInputs = new List<BaseInput>();
        protected List<BaseSelectListInput> BaseSelectListInputs = new List<BaseSelectListInput>();
        protected List<BaseListPart> BaseListParts = new List<BaseListPart>();
        protected List<IBasePropertyCardInput> BasePropertyCardInputs = new List<IBasePropertyCardInput>();

        protected BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
        protected BaseSelectListInput AddToBaseSelectListInputs { set { BaseSelectListInputs.Add(value); } }
        protected BaseListPart AddToBaseListParts { set { BaseListParts.Add(value); } }

        protected List<IBasePropertyCardInput> BaseInputExtensions = new List<IBasePropertyCardInput>();
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                EventServices = GetEventServices();

                TModelType = typeof(TModel);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[TModelType.Name];

                BaseInputExtensions = ServiceProvider.GetServices<IBasePropertyCardInput>().ToList();

                SetUpDisplayLists(TModelType, GUIType.Card, ComponentModelInstance);
            });

            if (ShowEntryByStart)
            {
                var entry = await EntryToBeShownByStart?.Invoke(EventServices);
                await ShowAsync(entry == null, entry?.GetPrimaryKeys());
            }
        }

        protected virtual async Task<RenderFragment> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, bool isReadonly)
        {
            foreach (var baseinput in BaseInputExtensions)
                if (await baseinput.IsHandlingPropertyRenderingAsync(Model, displayItem, EventServices))
                    return GetBaseInputExtensionAsRenderFragment(displayItem, isReadonly, baseinput.GetType(), Model);

            return null;
        }
        protected RenderFragment GetBaseInputExtensionAsRenderFragment(DisplayItem displayItem, bool isReadonly, Type baseInputExtensionType, IBaseModel model) => builder =>
         {
             builder.OpenComponent(0, baseInputExtensionType);

             builder.AddAttribute(1, "Model", model);
             builder.AddAttribute(2, "Property", displayItem.Property);
             builder.AddAttribute(3, "ReadOnly", isReadonly);
             builder.AddAttribute(4, "Service", Service);
             builder.AddAttribute(5, "ModelLocalizer", ModelLocalizer);

             builder.AddAttribute(6, "OnBeforeConvertPropertyType", EventCallback.Factory.Create<OnBeforeConvertPropertyTypeArgs>(this, (args) => OnBeforeConvertPropertyType.InvokeAsync(args)));
             builder.AddAttribute(7, "OnBeforePropertyChanged", EventCallback.Factory.Create<OnBeforePropertyChangedArgs>(this, (args) => OnBeforePropertyChanged.InvokeAsync(args)));
             builder.AddAttribute(8, "OnAfterPropertyChanged", EventCallback.Factory.Create<OnAfterPropertyChangedArgs>(this, (args) => OnAfterPropertyChanged.InvokeAsync(args)));

             builder.AddComponentReferenceCapture(9, (input) => BasePropertyCardInputs.Add((IBasePropertyCardInput)input));

             builder.CloseComponent();
         };

        #endregion

        #region Actions
        public async Task ShowAsync(bool addingMode, params object[] primaryKeys)
        {
            await Service.RefreshDbContextAsync();

            ModelLoaded = false;
            AddingMode = addingMode;
            BaseInputs.Clear();
            BaseSelectListInputs.Clear();
            BaseListParts.Clear();
            BasePropertyCardInputs.Clear();
            ResetInvalidFeedback();

            if (AddingMode)
            {
                Model = new TModel();
                var args = new OnCreateNewEntryInstanceArgs(Model, EventServices);
                await OnCreateNewEntryInstance.InvokeAsync(args);
                await Model.OnCreateNewEntryInstance(args);
            }
            else
                Model = await Service.GetWithAllNavigationPropertiesAsync<TModel>(primaryKeys); //Load all properties so the dbcontext dont load entries via lazy loading in parallel and crash

            if (Model == null)
                throw new CRUDException(Localizer["Can not find Entry with the Primarykeys {0} for displaying in Card", String.Join(", ", primaryKeys)]);

            await PrepareForeignKeyProperties(Service, Model);
            await PrepareCustomLookupData(Model, EventServices);

            ValidationContext = new ValidationContext(Model, ServiceProvider, new Dictionary<object, object>()
            {
                [typeof(IStringLocalizer)] = ModelLocalizer,
                [typeof(BaseService)] = Service
            });

            Model.OnReloadEntityFromDatabase += async (sender, e) => await Entry_OnReloadEntityFromDatabase(sender, e);
            ModelLoaded = true;
        }

        protected async Task Entry_OnReloadEntityFromDatabase(object sender, EventArgs e)
        {
            if (Model == null)
                return;

            await ShowAsync(false, Model.GetPrimaryKeys());

            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public virtual async Task<bool> SaveCardAsync(bool showSnackBar = true)
        {
            ResetInvalidFeedback();

            if (!CardIsValid())
                return false;

            var success = true;
            try
            {
                if (AddingMode)
                {
                    var args = new OnBeforeAddEntryArgs(Model, false, EventServices);
                    await OnBeforeAddEntry.InvokeAsync(args);
                    await Model.OnBeforeAddEntry(args);
                    if (args.AbortAdding)
                        return false;

                    var dbEntry = Service.DbContext.Entry(Model);
                    if (dbEntry.State != EntityState.Added && !await Service.AddEntryAsync(Model))
                    {
                        ShowFormattedInvalidFeedback(Localizer["EntryAlreadyExistError", Model.GetPrimaryKeysAsString()]);
                        if (showSnackBar)
                            Snackbar.Show();
                        return false;
                    }

                    var onAfterArgs = new OnAfterAddEntryArgs(Model, EventServices);
                    await OnAfterAddEntry.InvokeAsync(onAfterArgs);
                    await Model.OnAfterAddEntry(onAfterArgs);
                }
                else
                {
                    var args = new OnBeforeUpdateEntryArgs(Model, false, EventServices);
                    await OnBeforeUpdateEntry.InvokeAsync(args);
                    await Model.OnBeforeUpdateEntry(args);
                    if (args.AbortUpdating)
                        return false;

                    Service.UpdateEntry(Model);

                    var onAfterArgs = new OnAfterUpdateEntryArgs(Model, EventServices);
                    await OnAfterUpdateEntry.InvokeAsync(onAfterArgs);
                    await Model.OnAfterUpdateEntry(onAfterArgs);
                }

                await Service.SaveChangesAsync();
                AddingMode = false;
                await InvokeOnAfterSaveChangesEvents(EventServices);
            }
            catch (CRUDException e)
            {
                ShowFormattedInvalidFeedback(ErrorHandler.PrepareExceptionErrorMessage(e));
                success = false;
            }
            catch (Exception e)
            {
                ShowFormattedInvalidFeedback(Localizer["UnknownSavingError", ErrorHandler.PrepareExceptionErrorMessage(e)]);
                success = false;
            }

            if (showSnackBar)
                Snackbar.Show();
            return success;
        }

        public void ResetCard()
        {
            BaseInputs?.Clear();
            BaseSelectListInputs?.Clear();
            BaseListParts?.Clear();
            ForeignKeyProperties = null;
            CachedForeignKeys = new Dictionary<Type, List<KeyValuePair<string, string>>>();
            Model = null;
            ModelLoaded = false;
        }

        public TModel GetCurrentModel()
        {
            return Model;
        }

        public async Task StateHasChangedAsync()
        {
            await InvokeAsync(() => StateHasChanged());
        }
        #endregion

        #region Events
        protected virtual async Task InvokeOnAfterSaveChangesEvents(EventServices eventServices)
        {
            var onAfterSaveChangesArgs = new OnAfterCardSaveChangesArgs(Model, false, eventServices);
            await OnAfterSaveChanges.InvokeAsync(onAfterSaveChangesArgs);
            await Model.OnAfterCardSaveChanges(onAfterSaveChangesArgs);

            onAfterSaveChangesArgs = new OnAfterCardSaveChangesArgs(Model, true, eventServices);
            foreach (PropertyInfo property in TModelType.GetIBaseModelProperties())
            {
                if (property.IsListProperty())
                {
                    if (property.GetValue(Model) is IList list)
                        foreach (IBaseModel item in list)
                            if (item != null)
                                await item.OnAfterCardSaveChanges(onAfterSaveChangesArgs);
                }
                else
                {
                    if (property.GetValue(Model) is IBaseModel value)
                        await value.OnAfterCardSaveChanges(onAfterSaveChangesArgs);
                }
            }
        }
      
        #endregion
       

        #region Validation
        public bool HasUnsavedChanges()
        {
            return Service.HasUnsavedChanges();
        }

        protected virtual bool CardIsValid()
        {
            var valid = true;

            foreach (var input in BaseInputs)
                if (!input.ValidatePropertyValue())
                    valid = false;

            foreach (var input in BaseSelectListInputs)
                if (!input.ValidatePropertyValue())
                    valid = false;

            foreach (var listPart in BaseListParts)
                if (!listPart.ListPartIsValid())
                    valid = false;

            foreach (var basePropertyCardInput in BasePropertyCardInputs)
                if (!basePropertyCardInput.ValidatePropertyValue())
                    valid = false;

            if (!Model.TryValidate(out List<ValidationResult> validationResults, ValidationContext))
                valid = false;

            if (!valid)
            {
                ShowFormattedInvalidFeedback(Localizer["Some properties are not valid"]);
                Snackbar.Show();
            }

            return valid;
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
