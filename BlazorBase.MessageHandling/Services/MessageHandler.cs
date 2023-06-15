using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Interfaces.IMessageHandler;
using static BlazorBase.MessageHandling.Models.ShowDateSpanDialogArgs;
using static BlazorBase.MessageHandling.Models.ShowFileInputDialogArgs;
using static BlazorBase.MessageHandling.Models.ShowTextInputDialogArgs;

namespace BlazorBase.MessageHandling.Services;

public class MessageHandler : IMessageHandler
{
    #region Events

    public event ShowMessageEventHandler? OnShowMessage;
    public event ShowConfirmDialogHandler? OnShowConfirmDialog;

    public event ShowDateSpanDialogHandler? OnShowDateSpanDialog;
    public event ShowFileInputDialogHandler? OnShowFileInputDialog;
    public event ShowTextInputDialogHandler? OnShowTextInputDialog;

    public event ShowLoadingMessageHandler? OnShowLoadingMessage;
    public event UpdateLoadingMessageHandler? OnUpdateLoadingMessage;
    public event CloseLoadingMessageHandler? OnCloseLoadingMessage;
    public event ShowLoadingProgressMessageHandler? OnShowLoadingProgressMessage;
    public event UpdateLoadingProgressMessageHandler? OnUpdateLoadingProgressMessage;
    public event CloseLoadingProgressMessageHandler? OnCloseLoadingProgressMessage;
    #endregion

    #region ShowMessage
    public void ShowMessage(string title, string message,
                           MessageType messageType = MessageType.Information,
                           Func<ModalClosingEventArgs, Task>? onClosing = null,
                           object? icon = null,
                           string? closeButtonText = null,
                           Color closeButtonColor = Color.Secondary,
                           ModalSize modalSize = ModalSize.Large)
    {
        ShowMessage(new ShowMessageArgs(title, message, messageType, onClosing, icon, closeButtonText, closeButtonColor, modalSize));
    }

    public void ShowMessage(ShowMessageArgs args)
    {
        OnShowMessage?.Invoke(args);
    }
    #endregion

    #region ShowConfirmDialog
    public void ShowConfirmDialog(string title, string message,
                            MessageType messageType = MessageType.Information,
                            Func<ModalClosingEventArgs, ConfirmDialogResult, Task>? onClosing = null,
                            object? icon = null,
                            string? confirmButtonText = null,
                            Color confirmButtonColor = Color.Primary,
                            string? abortButtonText = null,
                            Color abortButtonColor = Color.Secondary,
                            ModalSize modalSize = ModalSize.Large)
    {
        ShowConfirmDialog(new ShowConfirmDialogArgs(title, message, messageType, onClosing, icon, confirmButtonText, confirmButtonColor, abortButtonText, abortButtonColor, modalSize));
    }

    public void ShowConfirmDialog(ShowConfirmDialogArgs args)
    {
        OnShowConfirmDialog?.Invoke(args);
    }
    #endregion

    #region ShowDateSpanDialog
    public void ShowDateSpanDialog(
        string title,
        string message,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        DateInputMode dateInputMode = DateInputMode.Date,
        bool useAsSingleDatePicker = false,
        string? fromDateCaption = null,
        string? toDateCaption = null,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, ConfirmDialogResult, DateSpanDialogResult, Task>? onClosing = null,
        object? icon = null,
        string? confirmButtonText = null,
        Color confirmButtonColor = Color.Primary,
        string? abortButtonText = null,
        Color abortButtonColor = Color.Secondary,
        ModalSize modalSize = ModalSize.Large)
    {
        ShowDateSpanDialog(
           new ShowDateSpanDialogArgs(
               title,
               message,
               fromDate,
               toDate,
               dateInputMode,
               useAsSingleDatePicker,
               fromDateCaption,
               toDateCaption,
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

    public void ShowDateSpanDialog(ShowDateSpanDialogArgs args)
    {
        OnShowDateSpanDialog?.Invoke(args);
    }
    #endregion

    #region ShowFileInputDialog
    public void ShowFileInputDialog(
        string title,
        string message,
        string fileInputCaption,
        ulong? maxFileSize,
        string fileFilter,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, ConfirmDialogResult, FileInputDialogResult, Task>? onClosing = null,
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
        OnShowFileInputDialog?.Invoke(args);
    }
    #endregion

    #region Show Text Input Dialog
    public void ShowTextInputDialog(
    string title,
    string message,
    string? textInputCaption = null,
    MessageType messageType = MessageType.Information,
    Func<ModalClosingEventArgs, ConfirmDialogResult, TextInputDialogResult, Task>? onClosing = null,
    object? icon = null,
    string? confirmButtonText = null,
    Color confirmButtonColor = Color.Primary,
    string? abortButtonText = null,
    Color abortButtonColor = Color.Secondary,
    ModalSize modalSize = ModalSize.Large)
    {
        ShowTextInputDialog(
           new ShowTextInputDialogArgs(
               title,
               message,
               textInputCaption,
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

    public void ShowTextInputDialog(ShowTextInputDialogArgs args)
    {
        OnShowTextInputDialog?.Invoke(args);
    }
    #endregion

    #region ShowLoadingMessage
    public Guid ShowLoadingMessage(string message, RenderFragment? loadingChildContent = null)
    {
        return OnShowLoadingMessage?.Invoke(new ShowLoadingMessageArgs(message, loadingChildContent)) ?? Guid.Empty;
    }

    public bool UpdateLoadingMessage(Guid id, string message, RenderFragment? loadingChildContent = null)
    {
        return OnUpdateLoadingMessage?.Invoke(new ShowLoadingMessageArgs(message, loadingChildContent) { Id = id }) ?? false;
    }
    public bool CloseLoadingMessage(Guid id)
    {
        return OnCloseLoadingMessage?.Invoke(id) ?? false;
    }
    #endregion

    #region ShowLoadingProgressMessage
    public Guid ShowLoadingProgressMessage(string message,
                                           int currentProgress = 0,
                                           string? progressText = null,
                                           bool showProgressInText = true,
                                           RenderFragment? loadingChildContent = null)
    {
        return OnShowLoadingProgressMessage?.Invoke(
            new ShowLoadingProgressMessageArgs(
                message,
                currentProgress,
                progressText,
                showProgressInText,
                loadingChildContent)
            ) ?? Guid.Empty;
    }

    public bool UpdateLoadingProgressMessage(Guid id,
                                             string message,
                                             int currentProgress = 0,
                                             string? progressText = null,
                                             bool showProgressInText = true,
                                             RenderFragment? loadingChildContent = null)
    {
        return OnUpdateLoadingProgressMessage?.Invoke(
           new ShowLoadingProgressMessageArgs(
               message,
               currentProgress,
               progressText,
               showProgressInText,
               loadingChildContent)
           { Id = id }) ?? false;
    }

    public bool CloseLoadingProgressMessage(Guid id)
    {
        return OnCloseLoadingProgressMessage?.Invoke(id) ?? false;
    }
    #endregion
}
