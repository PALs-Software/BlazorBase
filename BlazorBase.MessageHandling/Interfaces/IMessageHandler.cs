using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Models;
using Blazorise;
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
    public delegate void ShowMessageEventHandler(ShowMessageArgs args);
    public event ShowMessageEventHandler OnShowMessage;
    public void ShowMessage(
        string title,
        string message,
        MessageType messageType = MessageType.Information,
        Func<ModalClosingEventArgs, Task>? onClosing = null,
        object? icon = null,
        string? closeButtonText = null,
        Color closeButtonColor = Color.Secondary,
        ModalSize modalSize = ModalSize.Large);
    public void ShowMessage(ShowMessageArgs args);
    #endregion

    #region Show Confirm Dialog
    public delegate void ShowConfirmDialogHandler(ShowConfirmDialogArgs args);
    public event ShowConfirmDialogHandler OnShowConfirmDialog;
    public void ShowConfirmDialog(
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
    public void ShowConfirmDialog(ShowConfirmDialogArgs args);
    #endregion

    #region Show Date Span Dialog
    public delegate void ShowDateSpanDialogHandler(ShowDateSpanDialogArgs args);
    public event ShowDateSpanDialogHandler OnShowDateSpanDialog;
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
        ModalSize modalSize = ModalSize.Large);
    public void ShowDateSpanDialog(ShowDateSpanDialogArgs args);
    #endregion

    #region Show File Input Dialog
    public delegate void ShowFileInputDialogHandler(ShowFileInputDialogArgs args);
    public event ShowFileInputDialogHandler OnShowFileInputDialog;
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
        ModalSize modalSize = ModalSize.Large);
    public void ShowFileInputDialog(ShowFileInputDialogArgs args);
    #endregion

    #region Show Text Input Dialog
    public delegate void ShowTextInputDialogHandler(ShowTextInputDialogArgs args);
    public event ShowTextInputDialogHandler OnShowTextInputDialog;
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
        ModalSize modalSize = ModalSize.Large);
    public void ShowTextInputDialog(ShowTextInputDialogArgs args);
    #endregion

    #region Show Loading Message
    public delegate Guid ShowLoadingMessageHandler(ShowLoadingMessageArgs args);
    public event ShowLoadingMessageHandler OnShowLoadingMessage;
    public Guid ShowLoadingMessage(string message, RenderFragment? loadingChildContent = null);

    public delegate bool UpdateLoadingMessageHandler(ShowLoadingMessageArgs args);
    public event UpdateLoadingMessageHandler OnUpdateLoadingMessage;
    public bool UpdateLoadingMessage(Guid id, string message, RenderFragment? loadingChildContent = null);

    public delegate bool CloseLoadingMessageHandler(Guid id);
    public event CloseLoadingMessageHandler OnCloseLoadingMessage;
    public bool CloseLoadingMessage(Guid id);
    #endregion

    #region Show Loading Message
    public delegate Guid ShowLoadingProgressMessageHandler(ShowLoadingProgressMessageArgs args);
    public event ShowLoadingProgressMessageHandler OnShowLoadingProgressMessage;
    public Guid ShowLoadingProgressMessage(
        string message,
        int currentProgress = 0,
        string? progressText = null,
        bool showProgressInText = true,
        RenderFragment? loadingChildContent = null);

    public delegate bool UpdateLoadingProgressMessageHandler(ShowLoadingProgressMessageArgs args);
    public event UpdateLoadingProgressMessageHandler OnUpdateLoadingProgressMessage;
    public bool UpdateLoadingProgressMessage(
        Guid id,
        string message,
        int currentProgress = 0,
        string? progressText = null,
        bool showProgressInText = true,
        RenderFragment? loadingChildContent = null);

    public delegate bool CloseLoadingProgressMessageHandler(Guid id);
    public event CloseLoadingProgressMessageHandler OnCloseLoadingProgressMessage;
    public bool CloseLoadingProgressMessage(Guid id);
    #endregion
}
