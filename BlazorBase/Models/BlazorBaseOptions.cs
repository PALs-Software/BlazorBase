using System;

namespace BlazorBase.Models;
public class BlazorBaseOptions : IBlazorBaseOptions
{
    #region Constructors
    public BlazorBaseOptions(IServiceProvider serviceProvider, Action<IBlazorBaseOptions> configureOptions)
    {
        (this as IBlazorBaseOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties
    public BaseOptionsImportMode OptionsImportMode { get; set; }

    public Type OptionsImportFromDatabaseEntryType { get; set; }
    #endregion

}