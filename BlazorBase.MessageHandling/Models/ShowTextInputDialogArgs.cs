using BlazorBase.MessageHandling.Enum;
using Blazorise;
using System;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models;

public class ShowTextInputDialogArgs : ShowConfirmDialogArgs
{
    public ShowTextInputDialogArgs() { }
    public ShowTextInputDialogArgs(
        string title,
        string message,
        string? textInputCaption = null,
        bool maskText = false,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, ConfirmDialogResult, TextInputDialogResult, Task>? onClosing = null,
        object? icon = null,
        string? confirmButtonText = null,
        Color confirmButtonColor = Color.Primary,
        string? abortButtonText = null,
        Color abortButtonColor = Color.Secondary,
        ModalSize modalSize = ModalSize.Large) : base(title, message, messageType, null, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonColor, modalSize)
    {
        OnClosing = onClosing;
        TextInputCaption = textInputCaption;
        MaskText = maskText;
    }

    public string? TextInputCaption { get; set; }

    public string? Text { get; set; }
    public bool MaskText { get; set; }

    public record TextInputDialogResult(string? Text);
    public new Func<ModalClosingEventArgs, ConfirmDialogResult, TextInputDialogResult, Task>? OnClosing { get; set; }
}
