using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Models;
using Blazorise;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Models.ShowDateSpanDialogArgs;
using static BlazorBase.MessageHandling.Models.ShowFileInputDialogArgs;
using static BlazorBase.MessageHandling.Models.ShowTextInputDialogArgs;

namespace BlazorBase.MessageHandling.Interfaces;

public interface IMessageHandler
{
    #region Show Message
    delegate void ShowMessageEventHandler(ShowMessageArgs args);
    event ShowMessageEventHandler OnShowMessage;
    void ShowMessage(
        string title,
        string message,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, Task>? onClosing = null,
        object? icon = null,
        string? closeButtonText = null,
        Color closeButtonColor = Color.Secondary,
        ModalSize modalSize = ModalSize.Large);
    void ShowMessage(ShowMessageArgs args);
    #endregion

    #region Show Confirm Dialog
    delegate void ShowConfirmDialogHandler(ShowConfirmDialogArgs args);
    event ShowConfirmDialogHandler OnShowConfirmDialog;
    void ShowConfirmDialog(
        string title,
        string message,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, ConfirmDialogResult, Task>? onClosing = null,
        object? icon = null,
        string? confirmButtonText = null,
        Color confirmButtonColor = Color.Primary,
        string? abortButtonText = null,
        Color abortButtonColor = Color.Secondary,
        ModalSize modalSize = ModalSize.Large);
    void ShowConfirmDialog(ShowConfirmDialogArgs args);
    #endregion

    #region Show Date Span Dialog
    delegate void ShowDateSpanDialogHandler(ShowDateSpanDialogArgs args);
    event ShowDateSpanDialogHandler OnShowDateSpanDialog;
    void ShowDateSpanDialog(
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
        ModalSize modalSize = ModalSize.Large);
    void ShowDateSpanDialog(ShowDateSpanDialogArgs args);
    #endregion

    #region Show File Input Dialog
    delegate void ShowFileInputDialogHandler(ShowFileInputDialogArgs args);
    event ShowFileInputDialogHandler OnShowFileInputDialog;
    void ShowFileInputDialog(
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
        ModalSize modalSize = ModalSize.Large);
    void ShowFileInputDialog(ShowFileInputDialogArgs args);
    #endregion

    #region Show Text Input Dialog
    delegate void ShowTextInputDialogHandler(ShowTextInputDialogArgs args);
    event ShowTextInputDialogHandler OnShowTextInputDialog;
    void ShowTextInputDialog(
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
        ModalSize modalSize = ModalSize.Large);
    void ShowTextInputDialog(ShowTextInputDialogArgs args);
    #endregion

    #region Show Loading Message
    delegate ulong ShowLoadingMessageHandler(ShowLoadingMessageArgs args);
    event ShowLoadingMessageHandler OnShowLoadingMessage;
    ulong ShowLoadingMessage(string message, RenderFragment? loadingChildContent = null);

    delegate bool UpdateLoadingMessageHandler(ShowLoadingMessageArgs args);
    event UpdateLoadingMessageHandler OnUpdateLoadingMessage;
    bool UpdateLoadingMessage(ulong id, string message, RenderFragment? loadingChildContent = null);

    delegate bool CloseLoadingMessageHandler(ulong id);
    event CloseLoadingMessageHandler OnCloseLoadingMessage;
    bool CloseLoadingMessage(ulong id);
    #endregion

    #region Show Loading Message
    delegate ulong ShowLoadingProgressMessageHandler(ShowLoadingProgressMessageArgs args);
    event ShowLoadingProgressMessageHandler OnShowLoadingProgressMessage;
    ulong ShowLoadingProgressMessage(
        string message,
        int currentProgress = 0,
        string? progressText = null,
        bool showProgressInText = true,
        RenderFragment? loadingChildContent = null,
        string? abortButtonText = null,
        Func<ulong, Task>? onAborting = null);

    delegate bool UpdateLoadingProgressMessageHandler(ShowLoadingProgressMessageArgs args);
    event UpdateLoadingProgressMessageHandler OnUpdateLoadingProgressMessage;
    bool UpdateLoadingProgressMessage(
        ulong id,
        string message,
        int currentProgress = 0,
        string? progressText = null,
        bool showProgressInText = true,
        RenderFragment? loadingChildContent = null);

    delegate bool CloseLoadingProgressMessageHandler(ulong id);
    event CloseLoadingProgressMessageHandler OnCloseLoadingProgressMessage;
    bool CloseLoadingProgressMessage(ulong id);
    #endregion

    #region Show Snackbar
    delegate void ShowSnackbarEventHandler(ShowSnackbarArgs args);
    event ShowSnackbarEventHandler OnShowSnackbar;
    void ShowSnackbar(string message,
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
                        Func<SnackbarClosedEventArgs, Task>? onClosing = null);
    void ShowSnackbar(ShowSnackbarArgs args);
    #endregion

}
