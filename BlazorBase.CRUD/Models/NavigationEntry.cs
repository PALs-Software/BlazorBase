namespace BlazorBase.CRUD.Models;

public class NavigationEntry
{
    public NavigationEntry(string name, string link, string icon, string? group = null, bool openInNewTab = false)
    {
        Name = name;
        Link = link;
        Icon = icon;
        Group = group;
        OpenInNewTab = openInNewTab;
    }

    public string? Name { get; set; }
    public string? Link { get; set; }
    public string? Icon { get; set; }
    public string? Group { get; set; }
    public bool OpenInNewTab { get; set; }
}
