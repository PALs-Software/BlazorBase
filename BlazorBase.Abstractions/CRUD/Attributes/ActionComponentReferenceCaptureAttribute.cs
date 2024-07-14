using BlazorBase.Abstractions.CRUD.Interfaces;

namespace BlazorBase.Abstractions.CRUD.Attributes;

public record ActionComponentReferenceCaptureAttribute(int Sequence, Action<object> Value) : IActionComponentAttribute;
