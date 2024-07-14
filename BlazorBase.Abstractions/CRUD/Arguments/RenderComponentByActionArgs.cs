using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;

namespace BlazorBase.Abstractions.CRUD.Arguments;

public class RenderComponentByActionArgs
{
    public Type ComponentType { get; set; } = null!;
    public List<IActionComponentAttribute> Attributes { get; set; } = [];
    public Func<object?, EventServices, IBaseModel?, object?, Task>? OnComponentRemoved { get; set; } = null;
}
