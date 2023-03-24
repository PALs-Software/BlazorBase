using BlazorBase.Models;
using System;

namespace BlazorBase.Files.Models;
public class BlazorBaseFileOptions : IBlazorBaseFileOptions
{
    public static BlazorBaseFileOptions Instance { get; private set; }

    #region Constructors
    public BlazorBaseFileOptions(IServiceProvider serviceProvider, Action<BlazorBaseFileOptions> configureOptions)
    {
        (this as IBlazorBaseFileOptions).ImportOptions(serviceProvider, configureOptions);

        Instance = this;
    }
    #endregion

    #region Properties

    public BaseOptionsImportMode OptionsImportMode { get; set; }

    public Type FileImplementationType { get; set; } = typeof(BaseFile);
    public string ControllerRoute { get; set; } = "api/BaseFile";
    public string FileStorePath { get; set; } = @"C:\BlazorBaseFileStore";
    public string TempFileStorePath { get; set; } = @"C:\BlazorBaseFileStore\Temp";
    public bool AutomaticallyDeleteOldTemporaryFiles { get; set; } = true;
    public uint DeleteTemporaryFilesOlderThanXSeconds { get; set; } = 60 * 60 * 24 * 7; // 7 days
    #endregion
}