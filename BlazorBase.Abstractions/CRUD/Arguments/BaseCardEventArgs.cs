using BlazorBase.Abstractions.CRUD.Structures;

namespace BlazorBase.Abstractions.CRUD.Arguments;

public record OnEntryToBeShownByStartArgs(EventServices EventServices)
{
    public OnEntryToBeShownByStartArgs(bool viewMode, EventServices eventServices) : this(eventServices) => ViewMode = viewMode;
    public bool ViewMode { get; set; } = false;
}