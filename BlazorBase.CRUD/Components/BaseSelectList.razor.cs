using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Blazorise;
using System;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseSelectList<TModel> : BaseList<TModel> where TModel : class, IBaseModel, new()
    {
        #region Parameters
        [Parameter] public string Title { get; set; }
        [Parameter] public string SelectButtonText { get; set; }
        [Parameter] public bool HideSelectButton { get; set; } = false;
        [Parameter] public bool RenderAdditionalActionsOutsideOfButtonGroup { get; set; } = false;
        [Parameter] public RenderFragment<TModel> AdditionalActions { get; set; } = null;
        #endregion

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

                if (String.IsNullOrEmpty(SelectButtonText))
                    SelectButtonText = @SelectListLocalizer["Select"];

                if (String.IsNullOrEmpty(Title))
                    Title = ModelLocalizer[$"{TModelType.Name}_Plural"];

                if (HideSelectButton)
                    RenderAdditionalActionsOutsideOfButtonGroup = true;
            });

            await PrepareForeignKeyProperties(Service);
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
