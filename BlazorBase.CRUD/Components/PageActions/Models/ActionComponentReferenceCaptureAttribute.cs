using BlazorBase.CRUD.Components.PageActions.Interfaces;
using System;

namespace BlazorBase.CRUD.Components.PageActions.Models;

public record ActionComponentReferenceCaptureAttribute(int Sequence, Action<object> Value) : IActionComponentAttribute;
