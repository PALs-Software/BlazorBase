using System.Collections;

namespace BlazorBase.CRUD.Extensions;

public static class ListExtension
{
    public static string GetExtendedHashCode(this IList list)
    {
        int itemsHashCode = 0;
        unchecked
        {
            foreach (var item in list)
                itemsHashCode += item.GetHashCode();
        }

        return $"{list.GetHashCode()}_{list.Count}_{itemsHashCode}";
    }

}
