using BlazorBase.Models;
using System;

namespace BlazorBase.RichTextEditor.Models;

public interface IBlazorBaseRichTextEditorOptions : IBaseOptions
{
    Type? ImageFileType { get; set; }

    bool ResizeBigImagesToMaxImageSize { get; set; }
    int MaxImageSize { get; set; }
}
