namespace BlazorBase.CRUD.Models;

public class NavigationEntry
{
    public NavigationEntry(string name, string link, string icon)
    {
        Name = name;
        Link = link;
        Icon = icon;
    }

    public string Name { get; set; }
    public string Link { get; set; }
    public string Icon { get; set; }
}
