using BlazorBase.Abstractions.CRUD.Enums;
using Blazorise;

namespace BlazorBase.CRUD.Extensions;

public static class PresentationDataTypeExtension
{
    public static DateInputMode ToDateInputModel(this PresentationDataType type)
    {
        if (type == PresentationDataType.Date)
            return DateInputMode.Date;

        if (type == PresentationDataType.DateTime)
            return DateInputMode.DateTime;

        return DateInputMode.DateTime;
    }
}
