using BlazorBase.MessageHandling.Enum;
using Blazorise;

namespace BlazorBase.MessageHandling.Models;

public class ModalInfo
{
    public ModalInfo(ShowMessageArgs args)
    {
        Args = args;
    }

    public ShowMessageArgs Args { get; set; }
    public ConfirmDialogResult? ConfirmDialogResult { get; set; } = null;
    public Modal? Modal { get; set; }
}
