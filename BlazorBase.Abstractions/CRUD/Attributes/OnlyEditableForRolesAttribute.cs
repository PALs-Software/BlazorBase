using BlazorBase.Abstractions.CRUD.Enums;

namespace BlazorBase.Abstractions.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class OnlyEditableForRolesAttribute : Attribute
{
    public OnlyEditableForRolesAttribute() { }

    public OnlyEditableForRolesAttribute(GUIType guiType, params string[] roles)
    {
        GUITypes = [guiType];
        Roles = roles;
    }

    public OnlyEditableForRolesAttribute(GUIType[] guiTypes, params string[] roles)
    {
        GUITypes = guiTypes;
        Roles = roles;
    }

    public OnlyEditableForRolesAttribute(params string[] roles)
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
