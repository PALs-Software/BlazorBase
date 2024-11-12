using BlazorBase.Models;

namespace BlazorBase.RichTextEditor.Models;

public interface IBlazorBaseRichTextEditorOptions : IBaseOptions
{
    bool ResizeBigImagesToMaxImageSize { get; set; }
    int MaxImageSize { get; set; }

    bool ChangeImagesInEditorToLocalFiles { get; set; }
}