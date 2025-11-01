using BlazorBase.Models;
using System;

namespace BlazorBase.RichTextEditor.Models;

public class BlazorBaseRichTextEditorOptions: IBlazorBaseRichTextEditorOptions
{
    #region Constructors
    public BlazorBaseRichTextEditorOptions(IServiceProvider serviceProvider, Action<BlazorBaseRichTextEditorOptions> configureOptions)
    {
        (this as IBlazorBaseRichTextEditorOptions).ImportOptions(serviceProvider, configureOptions);
    }
    #endregion

    #region Properties
    public BaseOptionsImportMode OptionsImportMode { get; set; }

    public bool ResizeBigImagesToMaxImageSize { get; set; } = false;
    public uint MaxImageSize { get; set; } = 1024;

    public bool ChangeImagesInEditorToLocalFiles { get; set; } = true;
    #endregion

}
