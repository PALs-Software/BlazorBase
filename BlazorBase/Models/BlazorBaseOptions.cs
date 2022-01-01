using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Models;

public class BlazorBaseOptions : IBlazorBaseOptions
{
    #region Members
    protected readonly IServiceProvider ServiceProvider;
    protected readonly Action<BlazorBaseOptions> ConfigureOptions;
    #endregion

    #region Constructors
    public BlazorBaseOptions(IServiceProvider serviceProvider, Action<BlazorBaseOptions> configureOptions)
    {
        this.ServiceProvider = serviceProvider;
        this.ConfigureOptions = configureOptions;

        this.ConfigureOptions?.Invoke(this);
    }
    #endregion

    #region Properties
    public string WebsiteName { get; set; } = "BlazorBase";
    #endregion
}

