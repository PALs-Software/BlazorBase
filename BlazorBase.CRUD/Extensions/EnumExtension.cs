using BlazorBase.Abstractions.CRUD.Enums;

namespace BlazorBase.CRUD.Extensions;

public static class EnumExtension
{
    public static SortDirection GetNextSortDirection(this SortDirection direction)
    {
        switch (direction)
        {
            case SortDirection.None:
                return SortDirection.Ascending;
            case SortDirection.Ascending:
                return SortDirection.Descending;
            default:
                return SortDirection.None;
        }
    }
}
