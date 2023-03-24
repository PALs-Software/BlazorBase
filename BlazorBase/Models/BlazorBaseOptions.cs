using System;

namespace BlazorBase.Models;
public class BlazorBaseOptions : IBlazorBaseOptions
{
    #region Constructors
    public BlazorBaseOptions(IServiceProvider serviceProvider, Action<BlazorBaseOptions> configureOptions)
    {
        (this as IBlazorBaseOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties

    public BaseOptionsImportMode OptionsImportMode { get; set; }

    public string WebsiteName { get; set; }
    public string ShortWebsiteName { get; set; }

    #endregion

}