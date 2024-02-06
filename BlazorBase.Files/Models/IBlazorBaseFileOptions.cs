using BlazorBase.Models;

namespace BlazorBase.Files.Models;
public interface IBlazorBaseFileOptions : IBaseOptions
{
    string ControllerRoute { get; set; }
    string FileStorePath { get; set; }
    string TempFileStorePath { get; set; }
    bool AutomaticallyDeleteOldTemporaryFiles { get; set; }
    uint DeleteTemporaryFilesOlderThanXSeconds { get; set; }

    bool UseImageThumbnails { get; set; }
    int ImageThumbnailSize { get; set; }
}