using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using BlazorBase.Services;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using static BlazorBase.MessageHandling.Models.ShowTextInputDialogArgs;

namespace BlazorBase.MessageHandling.Components;

public partial class TextInputDialogGenerator
{
    #region Properties
    protected ConcurrentDictionary<Guid, ModalInfo> ModalInfos { get; set; } = new();
    #endregion

    #region Injects
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<MessageGenerator> MessageGeneratorLocalizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<TextInputDialogGenerator> Localizer { get; set; } = null!;
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(() =>
        {
            MessageHandler.OnShowTextInputDialog += MessageHandler_OnShowTextInputDialog;
        });
    }

    private void MessageHandler_OnShowTextInputDialog(ShowTextInputDialogArgs args)
    {
        ShowTextInputDialog(args);
    }
    #endregion

    #region Methods
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
        if (args.IsHandled)
            return;

        InvokeAsync(() =>
        {
            args.IsHandled = true;

            args.ConfirmButtonText ??= MessageGeneratorLocalizer["Confirm"];
            args.CloseButtonText ??= MessageGeneratorLocalizer["Abort"];

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
        ModalInfos.TryRemove(id, out ModalInfo _);
    }

    protected async Task OnModalClosing(ModalInfo modalInfo, ModalClosingEventArgs args)
    {
        try
        {
            var textInputArgs = (ShowTextInputDialogArgs)modalInfo.Args;

            await (
                textInputArgs.OnClosing?.Invoke(
                    args,
                    modalInfo.ConfirmDialogResult ?? ConfirmDialogResult.Aborted,
                    new TextInputDialogResult(textInputArgs.Text)
                ) ?? Task.CompletedTask
            ); ;
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
}
