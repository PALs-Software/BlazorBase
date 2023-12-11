using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using BlazorBase.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Models.ShowFileInputDialogArgs;

namespace BlazorBase.MessageHandling.Components;

public partial class FileInputDialogGenerator
{
    #region Properties

    protected ConcurrentDictionary<Guid, ModalInfo> ModalInfos { get; set; } = new ConcurrentDictionary<Guid, ModalInfo>();
    #endregion

    #region Injects
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<MessageGenerator> MessageGeneratorLocalizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<FileInputDialogGenerator> Localizer { get; set; } = null!;
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(() =>
        {
            MessageHandler.OnShowFileInputDialog += MessageHandler_OnShowFileInputDialog;
        });
    }

    private void MessageHandler_OnShowFileInputDialog(ShowFileInputDialogArgs args)
    {
        ShowFileInputDialog(args);
    }
    #endregion

    #region Methods
    public void ShowFileInputDialog(
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
        ModalSize modalSize = ModalSize.Large)
    {
        ShowFileInputDialog(
            new ShowFileInputDialogArgs(
                title,
                message,
                fileInputCaption,
                maxFileSize,
                fileFilter,
                messageType,
                onClosing,
                icon,
                confirmButtonText,
                confirmButtonColor,
                abortButtonText,
                abortButtonColor,
                modalSize
            )
        );
    }

    public void ShowFileInputDialog(ShowFileInputDialogArgs args)
    {
        if (args.IsHandled)
            return;

        InvokeAsync(() =>
        {
            args.IsHandled = true;

            args.ConfirmButtonText ??= MessageGeneratorLocalizer["Confirm"];
            args.CloseButtonText ??= MessageGeneratorLocalizer["Abort"];

            args.FileInputCaption ??= Localizer["File:"];

            if (args.Icon == null)
                args.SetIconByMessageType();

            while (!ModalInfos.TryAdd(Guid.NewGuid(), new ModalInfo(args))) ;

            StateHasChanged();
        });
    }

    #endregion

    #region Modal
    protected void OnModalClosed(Guid id)
    {
        ModalInfos.TryRemove(id, out var _);
    }

    protected async Task OnModalClosing(ModalInfo modalInfo, ModalClosingEventArgs args)
    {
        try
        {
            var fileInputArgs = (ShowFileInputDialogArgs)modalInfo.Args;

            await (
                fileInputArgs.OnClosing?.Invoke(
                    args,
                    modalInfo.ConfirmDialogResult ?? ConfirmDialogResult.Aborted,
                    fileInputArgs.DialogResult
                ) ?? Task.CompletedTask
            );
        }
        catch (Exception e)
        {
            _ = Task.Run(() =>
            {
                MessageHandler.ShowMessage(MessageGeneratorLocalizer["Error"], ErrorHandler.PrepareExceptionErrorMessage(e), MessageType.Error);
            });
        }
    }

    protected void OnConfirmButtonClicked(ModalInfo modalInfo)
    {
        modalInfo.ConfirmDialogResult = ConfirmDialogResult.Confirmed;
        modalInfo.Modal?.Hide();
    }

    protected void OnAbortButtonClicked(ModalInfo modalInfo)
    {
        modalInfo.ConfirmDialogResult = ConfirmDialogResult.Aborted;
        modalInfo.Modal?.Hide();
    }

    #endregion

    #region Events
    protected async Task OnValueChangedAsync(FileChangedEventArgs fileChangedEventArgs, ModalInfo modalInfo)
    {
        var fileInputArgs = (ShowFileInputDialogArgs)modalInfo.Args;
        var maxFileSize = fileInputArgs.MaxFileSize;
        try
        {
            var files = ((FileChangedEventArgs)fileChangedEventArgs).Files;
            if (files.Length == 0)
                return;

            fileInputArgs.ShowLoadingIndicator = true;
            var file = files.First();
            if (maxFileSize != null && maxFileSize != 0 && (ulong)file.Size > maxFileSize)
                throw new IOException(Localizer["The file exceed the maximum allowed file size of {0} bytes", maxFileSize]);

            fileInputArgs.DialogResult = new FileInputDialogResult(file.Name, file.Size, await GetBytesFromFileStreamAsync(file));

            OnConfirmButtonClicked(modalInfo);

            SetValidation(fileInputArgs, showValidation: false);
        }
        catch (Exception e)
        {
            SetValidation(fileInputArgs, feedback: ErrorHandler.PrepareExceptionErrorMessage(e));
        }
        finally
        {
            fileInputArgs.ShowLoadingIndicator = false;
        }
    }

    protected async Task<byte[]?> GetBytesFromFileStreamAsync(IFileEntry file)
    {
        if (file.Size == 0)
            return null;

        using var memoryStream = new MemoryStream();
        await file.WriteToStreamAsync(memoryStream);

        return memoryStream.ToArray();
    }

    protected void OnUploadProgressed(FileProgressedEventArgs e, ShowFileInputDialogArgs args)
    {
        args.UploadProgress = (int)e.Percentage;
    }

    public void SetValidation(ShowFileInputDialogArgs args, bool showValidation = true, bool isValid = false, string feedback = "")
    {
        args.FeedbackClass = isValid ? "valid-feedback" : "invalid-feedback";
        if (!showValidation)
        {
            args.Feedback = args.InputClass = String.Empty;
            return;
        }

        args.Feedback = feedback;
        args.InputClass = isValid ? "is-valid" : "is-invalid";
    }
    #endregion
}
