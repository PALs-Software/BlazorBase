using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Blazorise;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseSelectList<TModel> : BaseList<TModel> where TModel : class, IBaseModel, new()
    {
        #region Injects
        [Inject] protected IStringLocalizer<BaseSelectList<TModel>> SelectListLocalizer { get; set; }
        #endregion

        #region Members
        protected Modal Modal = default!;
        protected TModel SelectedEntry = null;
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                TModelType = typeof(TModel);
                SetUpDisplayLists(TModelType, GUIType.List);

                SetDisplayNames();
                PropertyListDisplays = ServiceProvider.GetServices<IBasePropertyListDisplay>().ToList();
            });

            await PrepareForeignKeyProperties(TModelType, Service);
        }
        #endregion

        protected void SelectEntry(TModel entry)
        {
            SelectedEntry = entry;
            HideModal();
        }

        public TModel GetSelectedEntry()
        {
            return SelectedEntry;
        }

        public async Task ShowModalAsync()
        {
            SelectedEntry = null;
            await VirtualizeList.RefreshDataAsync();
            Modal.Show();
        }

        public void HideModal()
        {
            Modal.Hide();
        }

        public void OnModalClosing(ModalClosingEventArgs args)
        {
            InvokeAsync(async () => await OnCardClosed.InvokeAsync(null));
        }

    }
}
