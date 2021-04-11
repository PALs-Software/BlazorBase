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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
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

        #region List Events
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnCreateNewListEntryInstanceArgs> OnCreateNewListEntryInstance { get; set; }
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
        private TModel Entry;
        private Type TModelType;

        private bool AddingMode;

        protected ValidationContext ValidationContext;
        protected string SelectedPageActionGroup { get; set; }
        #endregion

        #region Property Infos
        protected List<BaseInput> BaseInputs = new List<BaseInput>();
        protected List<BaseInputSelectList> BaseInputSelectLists = new List<BaseInputSelectList>();

        protected BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
        protected BaseInputSelectList AddToBaseInputSelectLists { set { BaseInputSelectLists.Add(value); } }

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

                SetUpDisplayLists(TModelType, GUIType.Card);
            });
        }


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
                Entry = new TModel();
                var args = new OnCreateNewEntryInstanceArgs(Entry, eventServices);
                await OnCreateNewEntryInstance.InvokeAsync(args);
                await Entry.OnCreateNewEntryInstance(args);
            }
            else
                Entry = await Service.GetAsync<TModel>(primaryKeys);

            if (Entry == null)
                throw new CRUDException(Localizer["Can not find Entry with the Primarykeys {0} for displaying in Card", String.Join(", ", primaryKeys)]);

            ValidationContext = new ValidationContext(Entry, ServiceProvider, new Dictionary<object, object>()
            {
                [typeof(IStringLocalizer<TModel>)] = ModelLocalizer,
                [typeof(DbContext)] = Service.DbContext
            });

            Entry.PageActionGroups = Entry.InitializePageActions();
            SelectedPageActionGroup = Entry.PageActionGroups?.FirstOrDefault()?.Caption;

            Entry.OnReloadEntityFromDatabase += async (sender, e) => await Entry_OnReloadEntityFromDatabase(sender, e);
            Modal.Show();
        }
        protected async Task Entry_OnReloadEntityFromDatabase(object sender, EventArgs e)
        {
            if (Entry == null)
                return;

            await ShowAsync(false, Entry.GetPrimaryKeys());

            await InvokeAsync(() => {
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
                    var args = new OnBeforeAddEntryArgs(Entry, false, eventServices);
                    await OnBeforeAddEntry.InvokeAsync(args);
                    await Entry.OnBeforeAddEntry(args);
                    if (args.AbortAdding)
                        return;

                    if (!await Service.AddEntryAsync(Entry))
                    {
                        ShowFormattedInvalidFeedback(Localizer["EntryAlreadyExistError", Entry.GetPrimaryKeysAsString()]);
                        return;
                    }

                    var onAfterArgs = new OnAfterAddEntryArgs(Entry, eventServices);
                    await OnAfterAddEntry.InvokeAsync(onAfterArgs);
                    await Entry.OnAfterAddEntry(onAfterArgs);
                }
                else
                {
                    var args = new OnBeforeUpdateEntryArgs(Entry, false, eventServices);
                    await OnBeforeUpdateEntry.InvokeAsync(args);
                    await Entry.OnBeforeUpdateEntry(args);
                    if (args.AbortUpdating)
                        return;

                    Service.UpdateEntry(Entry);

                    var onAfterArgs = new OnAfterUpdateEntryArgs(Entry, eventServices);
                    await OnAfterUpdateEntry.InvokeAsync(onAfterArgs);
                    await Entry.OnAfterUpdateEntry(onAfterArgs);
                }

                await Service.SaveChangesAsync();
            }
            catch (CRUDException e)
            {
                ShowFormattedInvalidFeedback(e.Message);
                return;
            }
            catch (Exception e)
            {
                ShowFormattedInvalidFeedback(Localizer["UnknownSavingError", e.Message]);
                return;
            }

            Modal.Hide();
            await OnCardClosed.InvokeAsync(null);
            Entry = null;
        }

        protected async Task RejectModalAsync()
        {
            Modal.Hide();
            Entry = null;

            await OnCardClosed.InvokeAsync(null);
        }

        protected void OnModalClosing(ModalClosingEventArgs args)
        {
            if (args.CloseReason != CloseReason.UserClosing)
                Task.Run(async () => await RejectModalAsync());
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

            return Entry.TryValidate(out List<ValidationResult> validationResults, ValidationContext);
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
