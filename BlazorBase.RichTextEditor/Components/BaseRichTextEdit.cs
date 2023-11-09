using Blazorise;
using Blazorise.RichTextEdit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorBase.RichTextEditor.Components;

public class BaseRichTextEdit : RichTextEdit
{
    #region Injects
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    #endregion

    /// <summary>
    /// Gets the editor content as html asynchronous, but not waiting for rerendering as the default GetHtmlAsync method does.
    /// </summary>
    public async ValueTask<string> GetHtmlDirectlyAsync()
    {
        return await JSRuntime.InvokeAsync<string>($"blazoriseRichTextEdit.getHtml", EditorRef);        
    }
}
