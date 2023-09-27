using BlazorBase.CRUD.ViewModels;

namespace BlazorBase.CRUD.EventArguments;

public record OnEntryToBeShownByStartArgs(EventServices EventServices)
{
    public OnEntryToBeShownByStartArgs(bool viewMode, EventServices eventServices) : this(eventServices) => ViewMode = viewMode;
    public bool ViewMode { get; set; } = false;
}