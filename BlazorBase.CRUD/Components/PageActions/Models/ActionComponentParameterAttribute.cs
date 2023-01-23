#nullable enable

using BlazorBase.CRUD.Components.PageActions.Interfaces;

namespace BlazorBase.CRUD.Components.PageActions.Models;

public record ActionComponentParameterAttribute(int Sequence, string Name, object? Value) : IActionComponentAttribute;
