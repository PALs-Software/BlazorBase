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

namespace BlazorBase.CRUD.Components
{
    public partial class BaseList<TModel> where TModel : class, IBaseModel, new()
    {
        [Inject]
        public BaseService Service { get; set; }

        [Inject]
        private IStringLocalizer<TModel> ModelLocalizer { get; set; }

        [Inject]
        private GenericClassStringLocalizer GenericClassStringLocalizer { get; set; }
        private IStringLocalizer Localizer { get; set; }

        [CascadingParameter]
        protected IMessageHandler MessageHandler { get; set; }

        private string SingleDisplayName;
        private string PluralDisplayName;
        private string ConfirmDialogDeleteTitle;
        private string ConfirmDialogDeleteMessage;
        private List<string> ColumnCaptions = new List<string>();
        private List<TModel> Entries = new List<TModel>();
        private List<PropertyInfo> VisibleProperties = new List<PropertyInfo>();
        private Type TModelType;

        private BaseCard<TModel> BaseCard = default!;
        private ConfirmDialog ConfirmDialog = default!;

        protected override async Task OnInitializedAsync()
        {
            Localizer = GenericClassStringLocalizer.GetLocalizer(typeof(BaseList<TModel>));
            TModelType = typeof(TModel);
            VisibleProperties = TModelType.GetVisibleProperties(GUIType.List);
            SingleDisplayName = ModelLocalizer[TModelType.Name];
            PluralDisplayName = ModelLocalizer[$"{TModelType.Name}_Plural"];
            ConfirmDialogDeleteTitle = Localizer[nameof(ConfirmDialogDeleteTitle), SingleDisplayName];

            foreach (var property in VisibleProperties)
                ColumnCaptions.Add(ModelLocalizer[property.Name]);

            await LoadListDataAsync();
        }

        protected async Task LoadListDataAsync()
        {
            Entries = await Service.GetDataAsync<TModel>();
        }


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
        }

        protected async Task OnEntryAddedAsync(TModel entry)
        {
            await InvokeAsync(() =>
            {
                Entries.Add(entry);
            });
        }
    }
}
