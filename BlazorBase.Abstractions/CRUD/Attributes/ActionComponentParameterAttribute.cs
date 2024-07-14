using BlazorBase.Abstractions.CRUD.Interfaces;

namespace BlazorBase.Abstractions.CRUD.Attributes;

public record ActionComponentParameterAttribute(int Sequence, string Name, object? Value) : IActionComponentAttribute;
