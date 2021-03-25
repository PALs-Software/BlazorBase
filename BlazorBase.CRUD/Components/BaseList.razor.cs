using BlazorBase.Components;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
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
    public partial class BaseList<TModel> where TModel : class, IBaseModel, new()
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

        [CascadingParameter]
        protected IMessageHandler MessageHandler { get; set; }

        #endregion

        #region Members

        private string ConfirmDialogDeleteTitle;
        private string ConfirmDialogDeleteMessage;
        private List<string> ColumnCaptions = new List<string>();
        private List<TModel> Entries = new List<TModel>();
        private List<PropertyInfo> VisibleProperties = new List<PropertyInfo>();
        private Type TModelType;

        private BaseCard<TModel> BaseCard = default!;
        private ConfirmDialog ConfirmDialog = default!;
        #endregion

        #region Init

        protected override async Task OnInitializedAsync()
        {
            Localizer = GenericClassStringLocalizer.GetLocalizer(typeof(BaseList<TModel>));
            TModelType = typeof(TModel);
            VisibleProperties = TModelType.GetVisibleProperties(GUIType.List);

            if (String.IsNullOrEmpty(SingleDisplayName))
                SingleDisplayName = ModelLocalizer[TModelType.Name];
            if (String.IsNullOrEmpty(PluralDisplayName))
                PluralDisplayName = ModelLocalizer[$"{TModelType.Name}_Plural"];
            ConfirmDialogDeleteTitle = Localizer[nameof(ConfirmDialogDeleteTitle), SingleDisplayName];

            foreach (var property in VisibleProperties)
                ColumnCaptions.Add(ModelLocalizer[property.Name]);

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
            await BaseCard.Show(new TModel(), addingMode: true);
        }
        protected async Task EditEntryAsync(TModel entry)
        {
            await BaseCard.Show(entry);
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

            try
            {
                await Service.RemoveEntryAsync((TModel)args.Sender);
                await Service.SaveChangesAsync();
                Entries.Remove((TModel)args.Sender);
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

        protected async Task CardOnBeforeAddEntry(OnBeforeAddEntryArgs args)
        {
            await OnBeforeAddEntry.InvokeAsync(args);
        }

        protected async Task CardOnAfterAddEntry(OnAfterAddEntryArgs args)
        {
            await InvokeAsync(() =>
            {
                Entries.Add((TModel)args.Model);
            });

            await OnAfterAddEntry.InvokeAsync(args);
        }

        protected async Task CardOnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args)
        {
            await OnBeforeUpdateEntry.InvokeAsync(args);
        }

        protected async Task CardOnAfterUpdateEntry(OnAfterUpdateEntryArgs args)
        {
            await OnAfterUpdateEntry.InvokeAsync(args);
        }

        protected async Task CardOnBeforePropertyChanged(OnBeforePropertyChangedArgs args)
        {
            await OnBeforePropertyChanged.InvokeAsync(args);
        }

        protected async Task CardOnAfterPropertyChanged(OnAfterPropertyChangedArgs args)
        {
            await OnAfterPropertyChanged.InvokeAsync(args);
        }
        #endregion
    }
}
