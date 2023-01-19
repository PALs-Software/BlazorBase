using BlazorBase.CRUD.Components.PageActions.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.CRUD.Components.PageActions.Interfaces;

public interface IActionComponent
{
    ActionComponentArgs Args { get; set; }
    EventCallback ComponentCanBeRemoved { get; set; }
}
