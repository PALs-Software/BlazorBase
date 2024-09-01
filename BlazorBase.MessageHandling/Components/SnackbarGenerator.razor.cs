using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using BlazorBase.Services;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Components;

public partial class SnackbarGenerator
{
    #region Properties
    protected ConcurrentDictionary<Guid, ShowSnackbarArgs> Snackbars { get; set; } = [];
    #endregion

    #region Injects
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<SnackbarGenerator> Localizer { get; set; } = null!;
    #endregion

    #region Members
    protected SnackbarStack? SnackbarStack;
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(() =>
        {
            MessageHandler.OnShowSnackbar += MessageHandler_OnShowSnackbar;
        });
    }
    #endregion

    #region Message Handler
    protected void MessageHandler_OnShowSnackbar(ShowSnackbarArgs args)
    {
        ShowSnackbar(args);
    }
    #endregion

    #region Methods

    public void ShowSnackbar(ShowSnackbarArgs args)
    {
        if (args.IsHandled || SnackbarStack == null)
            return;

        InvokeAsync(() =>
        {
            args.IsHandled = true;

            Guid key;
            do
            {
                key = Guid.NewGuid();
            } while (!Snackbars.TryAdd(Guid.NewGuid(), args));

            var snackBarColor = args.MessageType switch
            {
                MessageType.Information => SnackbarColor.Info,
                MessageType.Success => SnackbarColor.Success,
                MessageType.Error => SnackbarColor.Danger,
                MessageType.Warning => SnackbarColor.Warning,
                _ => SnackbarColor.Secondary
            };

            SnackbarStack.PushAsync(args.Message, args.Title, snackBarColor, options =>
            {
                options.Key = key.ToString();
                options.IntervalBeforeClose = args.MillisecondsBeforeClose;

                options.ShowActionButton = args.ShowActionButton;
                options.ActionButtonText = args.ActionButtonText;
                options.ActionButtonIcon = args.ActionButtonIcon;
                options.ShowCloseButton = args.ShowCloseButton;
                options.CloseButtonText = args.CloseButtonText;
                options.CloseButtonIcon = args.CloseButtonIcon;

                options.MessageTemplate = args.MessageTemplate;
            });

            StateHasChanged();
        });
    }

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

    protected async void OnSnackbarClosed(SnackbarClosedEventArgs args)
    {
        try
        {
            Snackbars.Remove(Guid.Parse(args.Key), out var showSnackbarArgs);
            if (showSnackbarArgs != null && showSnackbarArgs.OnClosing != null)
                await showSnackbarArgs.OnClosing.Invoke(args);
        }
        catch (Exception e)
        {
            MessageHandler.ShowMessage(Localizer["Error"], ErrorHandler.PrepareExceptionErrorMessage(e), MessageType.Error);
        }
    }
    #endregion
}
