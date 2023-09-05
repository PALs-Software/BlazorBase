using BlazorBase.MessageHandling.Enum;
using Blazorise;
using System;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models;

public class ShowFileInputDialogArgs : ShowConfirmDialogArgs
{
    public ShowFileInputDialogArgs() { }
    public ShowFileInputDialogArgs(
        string title,
        string message,
        string fileInputCaption,
        ulong? maxFileSize,
        string fileFilter,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, ConfirmDialogResult, FileInputDialogResult?, Task>? onClosing = null,
        object? icon = null,
        string? confirmButtonText = null,
        Color confirmButtonColor = Color.Primary,
        string? abortButtonText = null,
        Color abortButtonColor = Color.Secondary,
        ModalSize modalSize = ModalSize.Large) : base(title, message, messageType, null, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonColor, modalSize)
    {
        OnClosing = onClosing;
        FileInputCaption = fileInputCaption;
        MaxFileSize = maxFileSize;
        FileFilter = fileFilter;
    }

    public string? FileInputCaption { get; set; } = null;
    public ulong? MaxFileSize { get; set; } = null;
    public string? FileFilter { get; set; } = null;

    public FileInputDialogResult? DialogResult { get; set; } = null;

    public bool ShowLoadingIndicator { get; set; } = false;
    public int UploadProgress { get; set; } = 0;

    public string? InputClass { get; set; }
    public string? FeedbackClass { get; set; }
    public string? Feedback { get; set; }

    public record FileInputDialogResult(string FileName, long Size, byte[]? File);
    public new Func<ModalClosingEventArgs, ConfirmDialogResult, FileInputDialogResult?, Task>? OnClosing { get; set; }
}
