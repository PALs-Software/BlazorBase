using BlazorBase.Abstractions.CRUD.Enums;

namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OnlyVisibleForRolesAttribute : Attribute
{
    public OnlyVisibleForRolesAttribute() { }

    public OnlyVisibleForRolesAttribute(GUIType guiType, params string[] roles)
    {
        GUITypes = [guiType];
        Roles = roles;
    }

    public OnlyVisibleForRolesAttribute(GUIType[] guiTypes, params string[] roles)
    {
        GUITypes = guiTypes;
        Roles = roles;
    }

    public OnlyVisibleForRolesAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public string[] Roles { get; set; } = [];

    public GUIType[] GUITypes { get; set; } =
    [
        GUIType.List,
        GUIType.ListPart,
        GUIType.Card
    ];
}
