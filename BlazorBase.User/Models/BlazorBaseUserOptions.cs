using BlazorBase.Models;
using System;
using System.Security;

namespace BlazorBase.User.Models;
public class BlazorBaseUserOptions : IBlazorBaseUserOptions
{
    #region Constructors
    public BlazorBaseUserOptions() { }
    public BlazorBaseUserOptions(IServiceProvider serviceProvider, Action<BlazorBaseUserOptions> configureOptions)
    {
        (this as IBlazorBaseUserOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties
    public BaseOptionsImportMode OptionsImportMode { get; set; }
    public Type OptionsImportFromDatabaseEntryType { get; set; } = default!;

    public string LoginPath { get; set; }
    #endregion


}
