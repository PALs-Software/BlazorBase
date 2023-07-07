#nullable enable

using BlazorBase.CRUD.Components.PageActions.Interfaces;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.PageActions.Models;

public class RenderComponentByActionArgs
{
    public Type ComponentType { get; set; } = null!;
    public List<IActionComponentAttribute> Attributes { get; set; } = new();
    public Func<object?, EventServices, IBaseModel?, object?, Task>? OnComponentRemoved { get; set; } = null;
}
