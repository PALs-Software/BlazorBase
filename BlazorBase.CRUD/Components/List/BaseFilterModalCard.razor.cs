using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.List
{
    public partial class BaseFilterModalCard<TModel> : BaseDisplayComponent, IDisposable where TModel : class, IBaseModel, new()
    {
        #region Parameters

        #region Events

        #region Model
        [Parameter] public EventCallback OnCardClosed { get; set; }
        #endregion

        #endregion

        [Parameter] public TModel ComponentModelInstance { get; set; }
        #endregion

        #region Injects

        [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; }
        [Inject] protected IStringLocalizer<BaseList<TModel>> Localizer { get; set; }

        #endregion

        #region Members
        protected Modal Modal = default!;

        protected List<string> VisibleEntries = new List<string>();
        protected List<string> InVisibleEntries = new List<string>();

        //protected string SelectedVisibleEntry = null;
        //protected string SelectedInVisibleEntry = null;
        #endregion

        #region Init

        public void ShowModal()
        {
            Modal.Show();
        }

        protected override async Task OnInitializedAsync()
        {
            
            await InvokeAsync(() =>
            {
                var tempModel = new TModel();
                VisibleEntries = tempModel.GetVisibleProperties(Enums.GUIType.List).Select(x => x.Name).ToList();
                if (ComponentModelInstance == null)
                    ComponentModelInstance = new TModel();
                InVisibleEntries = ComponentModelInstance.PropertyNamesToRemoveFromListView;
                VisibleEntries.RemoveAll(x => InVisibleEntries.Contains(x));
            });
        }

        public void Dispose()
        {
        }

        #endregion

        protected void OnVisibleSelectedItemChange(string name)
        {
            VisibleEntries.Remove(name);
            InVisibleEntries.Add(name);
        }

        protected void OnInVisibleSelectedItemChange(string name)
        {
            VisibleEntries.Add(name);
            InVisibleEntries.Remove(name);
        }

        protected async Task OnCloseModalAsync(ModalClosingEventArgs args)
        {
            await OnCardClosed.InvokeAsync(args);
        }
        public async Task HideModalAsync()
        {
            Modal.Hide();
            await OnCardClosed.InvokeAsync();
        }
    }
}
