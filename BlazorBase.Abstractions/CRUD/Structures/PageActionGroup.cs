using BlazorBase.Abstractions.CRUD.Enums;

namespace BlazorBase.Abstractions.CRUD.Structures;

public class PageActionGroup
{
    public string? Caption { get; set; }
    public string? ToolTip { get; set; }
    public object? Image { get; set; }
    public GUIType[] VisibleInGUITypes { get; set; } = [GUIType.Card, GUIType.List, GUIType.ListPart];
    public Func<EventServices, Task<bool>> Visible { get; set; } = x => Task.FromResult(true);
    public List<PageAction> PageActions { get; set; } = [];
    public bool PreventAutoRemovingByEmptyPageActions { get; set; } = false;

    public static class DefaultGroups
    {
        public static readonly string Process = "Process";
        public static readonly string RelatedInformations = "Related Informations";
    }
}
