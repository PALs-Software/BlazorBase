using BlazorBase.Components;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseList<TModel> : BaseDisplayComponent where TModel : class, IBaseModel, new()
    {
        #region Parameters

        #region Events
        [Parameter] public EventCallback OnCardClosed { get; set; }
        [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
        [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
        [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
        [Parameter] public EventCallback<OnBeforeRemoveEntryArgs> OnBeforeRemoveEntry { get; set; }
        [Parameter] public EventCallback<OnAfterRemoveEntryArgs> OnAfterRemoveEntry { get; set; }

        #region List Events
        [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
        [Parameter] public EventCallback<OnBeforeUpdateListEntryArgs> OnBeforeUpdateListEntry { get; set; }
        [Parameter] public EventCallback<OnAfterUpdateListEntryArgs> OnAfterUpdateListEntry { get; set; }
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

        [Inject]
        public BaseService Service { get; set; }

        [Inject]
        private IStringLocalizer<TModel> ModelLocalizer { get; set; }

        [Inject]
        private StringLocalizerFactory GenericClassStringLocalizer { get; set; }
        private IStringLocalizer Localizer { get; set; }

        [Inject]
        private IServiceProvider ServiceProvider { get; set; }

        [CascadingParameter]
        protected IMessageHandler MessageHandler { get; set; }

        #endregion

        #region Members

        private string ConfirmDialogDeleteTitle;
        private string ConfirmDialogDeleteMessage;
        private List<TModel> Entries = new List<TModel>();
        private Type TModelType;

        private BaseCard<TModel> BaseCard = default!;
        private ConfirmDialog ConfirmDialog = default!;
        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            Localizer = GenericClassStringLocalizer.GetLocalizer(typeof(BaseList<TModel>));
            TModelType = typeof(TModel);

            SetUpDisplayLists(TModelType, GUIType.List);

            if (String.IsNullOrEmpty(SingleDisplayName))
                SingleDisplayName = ModelLocalizer[TModelType.Name];
            if (String.IsNullOrEmpty(PluralDisplayName))
                PluralDisplayName = ModelLocalizer[$"{TModelType.Name}_Plural"];
            ConfirmDialogDeleteTitle = Localizer[nameof(ConfirmDialogDeleteTitle), SingleDisplayName];

            await LoadListDataAsync();
        }

        protected async Task LoadListDataAsync()
        {
            if (DataLoadCondition == null)
                Entries = await Service.GetDataAsync<TModel>();
            else
                Entries = await Service.GetDataAsync(DataLoadCondition);
        }

        #endregion

        #region CRUD

        protected async Task AddEntryAsync()
        {
            await BaseCard.Show(addingMode: true);
        }
        protected async Task EditEntryAsync(TModel entry)
        {
            await BaseCard.Show(addingMode: false, entry.GetPrimaryKeys());
        }

        protected async Task RemoveEntryAsync(TModel entry)
        {
            if (entry == null)
                return;

            var primaryKeyString = String.Join(", ", entry.GetPrimaryKeys());
            ConfirmDialogDeleteMessage = Localizer[nameof(ConfirmDialogDeleteMessage), primaryKeyString];

            await ConfirmDialog.Show(entry);
        }

        #endregion

        #region Modal Events

        protected async Task OnConfirmDialogClosedAsync(ConfirmDialogEventArgs args)
        {
            if (args.ConfirmDialogResult == ConfirmDialogResult.Aborted || args.Sender == null)
                return;

            var model = (TModel)args.Sender;
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
                MessageHandler.ShowMessage(Localizer["Error while deleting"], e.InnerException.Message);
            }
        }

        protected async Task OnCardClosedAsync()
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });

            await OnCardClosed.InvokeAsync();
        }

        protected async Task CardOnAfterAddEntry(OnAfterAddEntryArgs args)
        {
            await InvokeAsync(() =>
            {
                Entries.Add((TModel)args.Model);
            });

            await OnAfterAddEntry.InvokeAsync(args);
        }

        protected async Task CardOnAfterUpdateEntry(OnAfterUpdateEntryArgs args)
        {
            var primaryKeys = args.Model.GetPrimaryKeys();
            var entryIndex = Entries.FindIndex(entry => entry.GetPrimaryKeys().SequenceEqual(primaryKeys));
            Entries[entryIndex] = (TModel)args.Model;

            await OnAfterUpdateEntry.InvokeAsync(args);
        }     
        #endregion

        #region Other
        private EventServices GetEventServices()
        {
            return new EventServices()
            {
                ServiceProvider = ServiceProvider,
                Localizer = ModelLocalizer,
                BaseService = Service
            };
        }
        #endregion
    }
}
