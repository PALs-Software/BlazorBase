using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseModalCard<TModel> where TModel : class, IBaseModel, new()
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

        [Parameter]
        public string SingleDisplayName { get; set; }
        [Parameter] public bool ShowEntryByStart { get; set; }
        [Parameter] public Func<EventServices, Task<IBaseModel>> EntryToBeShownByStart { get; set; }

        #endregion

        #region Member
        protected Modal Modal = default!;
        protected BaseCard<TModel> BaseCard = default!;
        protected IStringLocalizer Localizer { get; set; }
        [Inject] protected StringLocalizerFactory StringLocalizerFactory { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                Localizer = StringLocalizerFactory.GetLocalizer(typeof(BaseCard<TModel>));
            });
        }
        #endregion

        public async Task ShowModalAsync(bool addingMode = false, params object[] primaryKeys)
        {
            await BaseCard.ShowAsync(addingMode, primaryKeys);
            Modal.Show();
        }

        public async Task SaveModalAsync()
        {
            await BaseCard.SaveCardAsync();
        }

        public async Task SaveAndCloseModalAsync()
        {
            await SaveModalAsync();
            await HideAsync();
        }

        public async Task HideAsync()
        {
            await RejectModalAsync();
        }

        public async Task RejectModalAsync()
        {
            Modal.Hide();
            BaseCard.ResetCard();

            await OnCardClosed.InvokeAsync(null);
        }

        public void OnModalClosing(ModalClosingEventArgs args)
        {
            if (args.CloseReason != CloseReason.UserClosing)
                Task.Run(async () => await RejectModalAsync());
        }
    }
}
