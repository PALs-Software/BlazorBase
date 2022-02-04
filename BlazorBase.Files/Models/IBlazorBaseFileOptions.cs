using BlazorBase.Models;

namespace BlazorBase.Files.Models;
public interface IBlazorBaseFileOptions : IBaseOptions
{
    string FileStorePath { get; set; }
    string TempFileStorePath { get; set; }
    bool AutomaticallyDeleteOldTemporaryFiles { get; set; }
    uint DeleteTemporaryFilesOlderThanXSeconds { get; set; }
}