using BlazorBase.Abstractions.CRUD.Attributes;

namespace BlazorBase.Abstractions.CRUD.Interfaces;

public interface IDisplayGroup
{
    VisibleAttribute GroupAttribute { get; set; }
    List<IDisplayItem> DisplayItems { get; set; }
}
