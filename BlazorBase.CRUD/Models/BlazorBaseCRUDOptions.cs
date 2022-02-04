using BlazorBase.Models;
using System;

namespace BlazorBase.CRUD.Models;
public class BlazorBaseCRUDOptions : IBlazorBaseCRUDOptions
{
    #region Constructors
    public BlazorBaseCRUDOptions(IServiceProvider serviceProvider, Action<IBlazorBaseCRUDOptions> configureOptions)
    {
        (this as IBlazorBaseCRUDOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties
    public BaseOptionsImportMode OptionsImportMode { get; set; }
    public Type OptionsImportFromDatabaseEntryType { get; set; } = default!;
    #endregion
}
