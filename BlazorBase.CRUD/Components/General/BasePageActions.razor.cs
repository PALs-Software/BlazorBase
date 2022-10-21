using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.General
{
    public partial class BasePageActions
    {
        #region Parameters

        #region Events
        [Parameter] public EventCallback<Exception> OnPageActionInvoked { get; set; }
        #endregion
        [Parameter] public IBaseModel BaseModel { get; set; }
        [Parameter] public Type BaseModelType { get; set; }
        [Parameter] public EventServices EventServices { get; set; }
        [Parameter] public IStringLocalizer ModelLocalizer { get; set; }
        [Parameter] public GUIType GUIType { get; set; }
        [Parameter] public bool ShowOnlyButtons { get; set; }
        #endregion

        #region Member
        protected List<PageActionGroup> PageActionGroups { get; set; }
        protected List<PageActionGroup> VisiblePageActionGroups { get; set; } = new List<PageActionGroup>();
        protected string SelectedPageActionGroup { get; set; }

        public IBaseModel OldBaseModel { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            await GeneratePageActionsAsync();

            if (BaseModel != null)
                BaseModel.OnRecalculateVisibilityStatesOfActions += BaseModel_OnRecalculateVisibilityStatesOfActions;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (OldBaseModel != BaseModel)
            {
                OldBaseModel = BaseModel;
                await GeneratePageActionsAsync();
            }   
        }

        #endregion

        #region Generate Page Actions

        protected async Task GeneratePageActionsAsync()
        {
            var instance = BaseModel;
            if (instance == null)
                instance = Activator.CreateInstance(BaseModelType) as IBaseModel;

            VisiblePageActionGroups.Clear();
            PageActionGroups = instance.GeneratePageActionGroups() ?? new List<PageActionGroup>();
            foreach (var group in PageActionGroups)
                if (group.VisibleInGUITypes.Contains(GUIType) && await group.Visible(EventServices))
                    VisiblePageActionGroups.Add(group);

            foreach (var group in VisiblePageActionGroups)
                foreach (var pageAction in group.PageActions.ToList())
                    if (!pageAction.VisibleInGUITypes.Contains(GUIType) || pageAction.ShowAsRowButtonInList != ShowOnlyButtons || !await pageAction.Visible(EventServices))
                        group.PageActions.Remove(pageAction);

            VisiblePageActionGroups.RemoveAll(group => group.PageActions.Count == 0);
            SelectedPageActionGroup = VisiblePageActionGroups.FirstOrDefault()?.Caption;
        }

        private void BaseModel_OnRecalculateVisibilityStatesOfActions(object sender, EventArgs e)
        {
            InvokeAsync(async () =>
            {
                await GeneratePageActionsAsync();
                StateHasChanged();
            });
        }
        #endregion

        #region Page Actions
        private void SelectedPageActionGroupChanged(string name)
        {
            SelectedPageActionGroup = name;
        }

        private async Task InvokePageAction(PageAction action)
        {
            Exception exception = null;
            try
            {
                await action.Action?.Invoke(EventServices, BaseModel);
            }
            catch (Exception e)
            {
                exception = e;
            }

            await OnPageActionInvoked.InvokeAsync(exception);
        }

        #endregion

    }
}
