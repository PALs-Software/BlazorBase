using BlazorBase.Models;

namespace BlazorBase.RichTextEditor.Models;

public interface IBlazorBaseRichTextEditorOptions : IBaseOptions
{
    bool ResizeBigImagesToMaxImageSize { get; set; }
    uint MaxImageSize { get; set; }
}
