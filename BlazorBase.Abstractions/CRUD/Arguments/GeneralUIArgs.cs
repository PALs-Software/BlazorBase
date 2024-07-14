using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;

namespace BlazorBase.Abstractions.CRUD.Arguments;

public record AdditionalHeaderPageActionsArgs(PageActionGroup PageActionGroup, object? Source, IBaseModel? Model, EventServices EventServices);
