using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseList<TModel> where TModel : BaseModel, new()
    {
        [Inject]
        public BaseService Service { get; set; }

        private string SingleDisplayName;
        private string PluralDisplayName;
        private string DeleteConfirmDialogTitle;
        private string DeleteConfirmDialogDeleteMessage;
        private List<string> ColumnCaptions = new List<string>();
        private List<TModel> Entries = new List<TModel>();
        private List<PropertyInfo> VisibleProperties = new List<PropertyInfo>();
        private Type TModelType;

        private BaseCard<TModel> BaseCard = default!;
        private ConfirmDialog ConfirmDialog = default!;

        protected override async Task OnInitializedAsync()
        {
            TModelType = typeof(TModel);
            VisibleProperties = TModelType.GetVisibleProperties(GUIType.List);
            SingleDisplayName = TModelType.GetDisplayName();
            PluralDisplayName = TModelType.GetPluralDisplayName();
            DeleteConfirmDialogTitle = $"{SingleDisplayName} löschen";

            foreach (var item in VisibleProperties)
                ColumnCaptions.Add(item.GetDisplayName());

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
            DeleteConfirmDialogDeleteMessage = $"Wollen Sie den Eintrag {primaryKeyString} wirklich löschen?";

            await ConfirmDialog.Show(entry);
        }

        protected async Task OnConfirmDialogClosedAsync(ConfirmDialogEventArgs args)
        {
            if (args.ConfirmDialogResult == ConfirmDialogResult.Aborted || args.Sender == null)
                return;

            Entries.Remove((TModel)args.Sender);
            await Service.RemoveEntryAsync((TModel)args.Sender);
            await Service.SaveChangesAsync();
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
