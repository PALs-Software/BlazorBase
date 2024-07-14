using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;

namespace BlazorBase.Abstractions.CRUD.Arguments;

public record ActionComponentArgs(object? Source, EventServices EventServices, IBaseModel? BaseModel);
