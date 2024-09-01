using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using Blazorise.Snackbar;
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

    public event ShowSnackbarEventHandler? OnShowSnackbar;
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
        OnShowFileInputDialog?.Invoke(args);
    }
    #endregion

    #region Show Text Input Dialog
    public void ShowTextInputDialog(
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
    ModalSize modalSize = ModalSize.Large)
    {
        ShowTextInputDialog(
           new ShowTextInputDialogArgs(
               title,
               message,
               textInputCaption,
               maskText,
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
    public ulong ShowLoadingMessage(string message, RenderFragment? loadingChildContent = null)
    {
        return OnShowLoadingMessage?.Invoke(new ShowLoadingMessageArgs(message, loadingChildContent)) ?? 0;
    }

    public bool UpdateLoadingMessage(ulong id, string message, RenderFragment? loadingChildContent = null)
    {
        return OnUpdateLoadingMessage?.Invoke(new ShowLoadingMessageArgs(message, loadingChildContent) { Id = id }) ?? false;
    }
    public bool CloseLoadingMessage(ulong id)
    {
        return OnCloseLoadingMessage?.Invoke(id) ?? false;
    }
    #endregion

    #region ShowLoadingProgressMessage
    public ulong ShowLoadingProgressMessage(string message,
                                           int currentProgress = 0,
                                           string? progressText = null,
                                           bool showProgressInText = true,
                                           RenderFragment? loadingChildContent = null,
                                           string? abortButtonText = null,
                                           Func<ulong, Task>? onAborting = null)
    {
        return OnShowLoadingProgressMessage?.Invoke(
            new ShowLoadingProgressMessageArgs(
                message,
                currentProgress,
                progressText,
                showProgressInText,
                loadingChildContent,
                abortButtonText,
                onAborting)
            ) ?? 0;
    }

    public bool UpdateLoadingProgressMessage(ulong id,
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

    public bool CloseLoadingProgressMessage(ulong id)
    {
        return OnCloseLoadingProgressMessage?.Invoke(id) ?? false;
    }
    #endregion

    #region Show Snackbar
    public void ShowSnackbar(string message,
                        string? title = null,
                        MessageType messageType = MessageType.Information,
                        double millisecondsBeforeClose = 2000,
                        bool showActionButton = false,
                        string? actionButtonText = null,
                        object? actionButtonIcon = null,
                        bool showCloseButton = false,
                        string? closeButtonText = null,
                        object? closeButtonIcon = null,
                        RenderFragment? messageTemplate = null,
                        Func<SnackbarClosedEventArgs, Task>? onClosing = null)
    {
        ShowSnackbar(new ShowSnackbarArgs(message, title, messageType, millisecondsBeforeClose, showActionButton, actionButtonText, actionButtonIcon, showCloseButton, closeButtonText, closeButtonIcon, messageTemplate, onClosing));
    }

    public void ShowSnackbar(ShowSnackbarArgs args)
    {
        OnShowSnackbar?.Invoke(args);
    }
    #endregion

}
