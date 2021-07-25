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

namespace BlazorBase.CRUD.Components
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
        #endregion

        #region Member
        protected List<PageActionGroup> PageActionGroups { get; set; }
        protected List<PageActionGroup> VisiblePageActionGroups { get; set; } = new List<PageActionGroup>();
        protected string SelectedPageActionGroup { get; set; }
        #endregion

        #region Init
        protected override async Task OnInitializedAsync()
        {
            var instance = BaseModel;
            if (instance == null)
                instance = Activator.CreateInstance(BaseModelType) as IBaseModel;

            PageActionGroups = instance.GeneratePageActionGroups() ?? new List<PageActionGroup>();
            foreach (var group in PageActionGroups)
                if (group.VisibleInGUITypes.Contains(GUIType) && await group.Visible(EventServices))
                    VisiblePageActionGroups.Add(group);

            foreach (var group in VisiblePageActionGroups)
                foreach (var pageAction in group.PageActions.ToList())
                    if (!pageAction.VisibleInGUITypes.Contains(GUIType) || !await pageAction.Visible(EventServices))
                        group.PageActions.Remove(pageAction);

            VisiblePageActionGroups.RemoveAll(group => group.PageActions.Count == 0);
            SelectedPageActionGroup = VisiblePageActionGroups.FirstOrDefault()?.Caption;
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
