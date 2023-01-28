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
    public Func<object?, EventServices, IBaseModel?, Task>? Action { get; set; } = null;
    public RenderComponentByActionArgs? RenderComponentByActionArgs { get; set; } = null;
}