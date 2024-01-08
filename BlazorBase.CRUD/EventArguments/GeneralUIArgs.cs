using BlazorBase.CRUD.Components.PageActions.Models;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;

namespace BlazorBase.CRUD.EventArguments;

public record AdditionalHeaderPageActionsArgs(PageActionGroup PageActionGroup, object? Source, IBaseModel? Model, EventServices EventServices);
