using BlazorBase.Models;

namespace BlazorBase.Server.User.Models;

public class BlazorBaseServerUserOptions : IBlazorBaseServerUserOptions
{
    #region Constructors
    public BlazorBaseServerUserOptions() { }
    public BlazorBaseServerUserOptions(IServiceProvider serviceProvider, Action<BlazorBaseServerUserOptions> configureOptions)
    {
        (this as IBlazorBaseServerUserOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties

    public BaseOptionsImportMode OptionsImportMode { get; set; }

    public bool LogUserSessions { get;set; }

    #endregion
}
