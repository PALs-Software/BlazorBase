#nullable enable

using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using Blazorise;
using System;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.PageActions.Models;

public class PageAction
{
    public string? Caption { get; set; }
    public string? ToolTip { get; set; }
    public Color Color { get; set; } = Color.Secondary;
    public object? Image { get; set; }
    public GUIType[] VisibleInGUITypes { get; set; } = new GUIType[] { GUIType.Card, GUIType.List, GUIType.ListPart };
    public bool ShowAsRowButtonInList { get; set; }
    public Func<EventServices, Task<bool>> Visible { get; set; } = x => Task.FromResult(true);

    /// <summary>
    /// 1. Parameter:<br/>
    /// Source object, like BaseList, BaseCard, BaseListPart <para/>
    /// 2. Parameter:<br/>
    /// Event services <para/>
    /// 3. Parameter:<br/>
    /// When List: The primary keys of the current selected object
    /// When BaseCard: The current base model instance 
    /// When ListPart: The current selected object 
    /// </summary>
    public Func<object?, EventServices, object?, Task>? Action { get; set; } = null;
    public RenderComponentByActionArgs? RenderComponentByActionArgs { get; set; } = null;
}