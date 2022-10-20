using BlazorBase.CRUD.Models;
using Blazorise;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseMenu
    {
        #region Parameters

        [Parameter]
        public object MenuName { get; set; }

        [Parameter]
        public object MenuIcon { get; set; }

        [Parameter]
        public bool DropDownIsVisibleByDefault { get; set; } = true;

        [Parameter]
        public List<NavigationEntry> NavigationEntries { get; set; } = new List<NavigationEntry>();

        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object> AdditionalAttributes { get; set; }

        #endregion

        #region Member
        protected bool DropDownIsVisible;
        #endregion

        #region Init

        protected override void OnInitialized()
        {
            DropDownIsVisible = DropDownIsVisibleByDefault;
        }

        protected void DropDownVisibleChanged(bool visible)
        {
            DropDownIsVisible = visible; // Update parameter value, so by rerendering the bardropdown component the right visibility state will be handed over
        }

        #endregion
    }
}
