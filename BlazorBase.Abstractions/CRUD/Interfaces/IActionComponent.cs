using BlazorBase.Abstractions.CRUD.Arguments;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.Abstractions.CRUD.Interfaces;

public interface IActionComponent
{
    ActionComponentArgs Args { get; set; }
    EventCallback ComponentCanBeRemoved { get; set; }
}
