using BlazorBase.Models;
using System;

namespace BlazorBase.Files.Models;
public interface IBlazorBaseFileOptions : IBaseOptions
{
    Type FileImplementationType { get; set; }
    string FileStorePath { get; set; }
    string TempFileStorePath { get; set; }
    bool AutomaticallyDeleteOldTemporaryFiles { get; set; }
    uint DeleteTemporaryFilesOlderThanXSeconds { get; set; }
}