using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBase.CRUD.Components.General;

public partial class BaseMenu
{
    #region Parameters

    [Parameter] public object MenuName { get; set; } = null!;
    [Parameter] public object MenuIcon { get; set; } = null!;
    [Parameter] public bool DropDownIsVisibleByDefault { get; set; } = true;
    [Parameter] public List<NavigationEntry> NavigationEntries { get; set; } = new List<NavigationEntry>();
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

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
