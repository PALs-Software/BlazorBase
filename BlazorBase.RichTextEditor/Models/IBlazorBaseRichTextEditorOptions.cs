using BlazorBase.Models;
using System;

namespace BlazorBase.RichTextEditor.Models;

public interface IBlazorBaseRichTextEditorOptions : IBaseOptions
{
    Type ImageFileType { get; set; }       
}
