using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.Modules;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseCard<TModel> : BaseDisplayComponent where TModel : class, IBaseModel, new()
    {
        #region Parameter

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

        [Inject]
        public BaseService Service { get; set; }

        [Parameter]
        public string SingleDisplayName { get; set; }

        #endregion

        #region Injects

        [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; }
        protected IStringLocalizer Localizer { get; set; }
        [Inject] protected IServiceProvider ServiceProvider { get; set; }
        [Inject] protected IMessageHandler MessageHandler { get; set; }
        #endregion

        #region Member
        private string Title;
        private MarkupString CardSummaryInvalidFeedback;
        private bool ShowInvalidFeedback = false;
        private Modal Modal = default!;
        private TModel Model;
        private Type TModelType;

        private bool AddingMode;

        protected ValidationContext ValidationContext;
        protected string SelectedPageActionGroup { get; set; }
        protected List<PageActionGroup> PageActionGroups { get; set; }
        #endregion

        #region Property Infos
        protected List<BaseInput> BaseInputs = new List<BaseInput>();
        protected List<BaseInputSelectList> BaseInputSelectLists = new List<BaseInputSelectList>();

        protected BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
        protected BaseInputSelectList AddToBaseInputSelectLists { set { BaseInputSelectLists.Add(value); } }

        protected List<IBaseInput> BaseInputExtensions = new List<IBaseInput>();
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Localizer = StringLocalizerFactory.GetLocalizer(typeof(BaseCard<TModel>));
                TModelType = typeof(TModel);

                if (String.IsNullOrEmpty(SingleDisplayName))
                    SingleDisplayName = ModelLocalizer[TModelType.Name];
                Title = Localizer[nameof(Title), SingleDisplayName];

                BaseInputExtensions = ServiceProvider.GetServices<IBaseInput>().ToList();

                SetUpDisplayLists(TModelType, GUIType.Card);
            });
        }

        protected async Task<RenderFragment> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem)
        {
            var eventServices = GetEventServices();

            foreach (var baseinput in BaseInputExtensions)
                if (await baseinput.IsHandlingPropertyRenderingAsync(Model, displayItem, eventServices))
                    return GetBaseInputExtensionAsRenderFragment(displayItem, baseinput.GetType());

            return null;
        }
        protected RenderFragment GetBaseInputExtensionAsRenderFragment(DisplayItem displayItem, Type baseInputExtensionType) => builder =>
        {
            builder.OpenComponent(0, baseInputExtensionType);

            builder.AddAttribute(1, "Model", Model);
            builder.AddAttribute(2, "Property", displayItem.Property);
            builder.AddAttribute(3, "ReadOnly", !AddingMode && displayItem.Property.IsKey());
            builder.AddAttribute(4, "Service", Service);
            builder.AddAttribute(5, "ModelLocalizer", ModelLocalizer);

            builder.AddAttribute(6, "OnBeforeConvertPropertyType", EventCallback.Factory.Create<OnBeforeConvertPropertyTypeArgs>(this, (args) => OnBeforeConvertPropertyType.InvokeAsync(args)));
            builder.AddAttribute(7, "OnBeforePropertyChanged", EventCallback.Factory.Create<OnBeforePropertyChangedArgs>(this, (args) => OnBeforePropertyChanged.InvokeAsync(args)));
            builder.AddAttribute(8, "OnAfterPropertyChanged", EventCallback.Factory.Create<OnAfterPropertyChangedArgs>(this, (args) => OnAfterPropertyChanged.InvokeAsync(args)));

            builder.CloseComponent();

        };
        #endregion

        #region Modal
        public async Task ShowAsync(bool addingMode = false, params object[] primaryKeys)
        {
            Service.RefreshDbContext();

            await PrepareForeignKeyProperties(TModelType, Service);
            AddingMode = addingMode;
            BaseInputs.Clear();
            BaseInputSelectLists.Clear();

            if (AddingMode)
            {
                var eventServices = GetEventServices();
                Model = new TModel();
                var args = new OnCreateNewEntryInstanceArgs(Model, eventServices);
                await OnCreateNewEntryInstance.InvokeAsync(args);
                await Model.OnCreateNewEntryInstance(args);
            }
            else
                Model = await Service.GetAsync<TModel>(primaryKeys);

            if (Model == null)
                throw new CRUDException(Localizer["Can not find Entry with the Primarykeys {0} for displaying in Card", String.Join(", ", primaryKeys)]);

            ValidationContext = new ValidationContext(Model, ServiceProvider, new Dictionary<object, object>()
            {
                [typeof(IStringLocalizer<TModel>)] = ModelLocalizer,
                [typeof(DbContext)] = Service.DbContext
            });

            PageActionGroups = Model.GeneratePageActionGroups() ?? new List<PageActionGroup>();
            SelectedPageActionGroup = PageActionGroups.FirstOrDefault()?.Caption;

            Model.OnReloadEntityFromDatabase += async (sender, e) => await Entry_OnReloadEntityFromDatabase(sender, e);
            Modal.Show();
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

        public async Task HideAsync()
        {
            await RejectModalAsync();
        }

        protected async Task SaveModalAsync()
        {
            ResetInvalidFeedback();

            if (!CardIsValid())
                return;

            var eventServices = GetEventServices();

            try
            {
                if (AddingMode)
                {
                    var args = new OnBeforeAddEntryArgs(Model, false, eventServices);
                    await OnBeforeAddEntry.InvokeAsync(args);
                    await Model.OnBeforeAddEntry(args);
                    if (args.AbortAdding)
                        return;

                    if (!await Service.AddEntryAsync(Model))
                    {
                        ShowFormattedInvalidFeedback(Localizer["EntryAlreadyExistError", Model.GetPrimaryKeysAsString()]);
                        return;
                    }

                    var onAfterArgs = new OnAfterAddEntryArgs(Model, eventServices);
                    await OnAfterAddEntry.InvokeAsync(onAfterArgs);
                    await Model.OnAfterAddEntry(onAfterArgs);
                }
                else
                {
                    var args = new OnBeforeUpdateEntryArgs(Model, false, eventServices);
                    await OnBeforeUpdateEntry.InvokeAsync(args);
                    await Model.OnBeforeUpdateEntry(args);
                    if (args.AbortUpdating)
                        return;

                    Service.UpdateEntry(Model);

                    var onAfterArgs = new OnAfterUpdateEntryArgs(Model, eventServices);
                    await OnAfterUpdateEntry.InvokeAsync(onAfterArgs);
                    await Model.OnAfterUpdateEntry(onAfterArgs);
                }

                await Service.SaveChangesAsync();
                await InvokeOnAfterSaveChangesEvents(eventServices);
            }
            catch (CRUDException e)
            {
                ShowFormattedInvalidFeedback(PrepareExceptionErrorMessage(e));
                return;
            }
            catch (Exception e)
            {
                ShowFormattedInvalidFeedback(Localizer["UnknownSavingError", PrepareExceptionErrorMessage(e)]);
                return;
            }

            Modal.Hide();
            await OnCardClosed.InvokeAsync(null);
            Model = null;
        }

        protected string PrepareExceptionErrorMessage(Exception e) {
            if (e.InnerException == null)
                return e.Message;

            return e.Message + Environment.NewLine + Environment.NewLine + Localizer["Inner Exception:"] + PrepareExceptionErrorMessage(e.InnerException);
        }

        protected async Task RejectModalAsync()
        {
            Modal.Hide();
            Model = null;

            await OnCardClosed.InvokeAsync(null);
        }

        protected void OnModalClosing(ModalClosingEventArgs args)
        {
            if (args.CloseReason != CloseReason.UserClosing)
                Task.Run(async () => await RejectModalAsync());
        }
        #endregion

        #region Events
        protected async Task InvokeOnAfterSaveChangesEvents(EventServices eventServices)
        {
            var onAfterSaveChangesArgs = new OnAfterSaveChangesArgs(Model, false, eventServices);
            await OnAfterSaveChanges.InvokeAsync(onAfterSaveChangesArgs);
            await Model.OnAfterSaveChanges(onAfterSaveChangesArgs);

            onAfterSaveChangesArgs = new OnAfterSaveChangesArgs(Model, true, eventServices);
            foreach (PropertyInfo property in TModelType.GetIBaseModelProperties())
            {
                if (property.IsListProperty())
                {
                    if (property.GetValue(Model) is IList list)
                        foreach (IBaseModel item in list)
                            if (item != null)
                                await item.OnAfterSaveChanges(onAfterSaveChangesArgs);
                }
                else
                {
                    if (property.GetValue(Model) is IBaseModel value)
                        await value.OnAfterSaveChanges(onAfterSaveChangesArgs);
                }
            }
        }
        #endregion

        #region Page Actions
        private void SelectedPageActionGroupChanged(string name)
        {
            SelectedPageActionGroup = name;
        }

        private async Task InvokePageAction(PageAction action)
        {
            ResetInvalidFeedback();

            try
            {
                await action.Action?.Invoke(GetEventServices());
            }
            catch (Exception e)
            {
                ShowFormattedInvalidFeedback(e.Message);
            }

            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        #endregion

        #region Validation
        protected virtual bool CardIsValid()
        {
            foreach (var input in BaseInputs)
                input.ValidatePropertyValue();

            foreach (var input in BaseInputSelectLists)
                input.ValidatePropertyValue();

            return Model.TryValidate(out List<ValidationResult> validationResults, ValidationContext);
        }

        private void ShowFormattedInvalidFeedback(string feedback)
        {
            CardSummaryInvalidFeedback = MarkupStringValidator.GetWhiteListedMarkupString(feedback);
            ShowInvalidFeedback = true;
        }

        private void ResetInvalidFeedback()
        {
            CardSummaryInvalidFeedback = (MarkupString)String.Empty;
            ShowInvalidFeedback = false;
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
