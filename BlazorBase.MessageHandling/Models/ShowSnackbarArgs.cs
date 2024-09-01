using BlazorBase.MessageHandling.Enum;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models;

public class ShowSnackbarArgs(string message,
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
    public string Message { get; set; } = message;
    public string? Title { get; set; } = title;
    public MessageType MessageType { get; set; } = messageType;

    public double MillisecondsBeforeClose { get; set; } = millisecondsBeforeClose;

    public bool ShowActionButton { get; set; } = showActionButton;
    public string? ActionButtonText { get; set; } = actionButtonText;
    public object? ActionButtonIcon { get; set; } = actionButtonIcon;

    public bool ShowCloseButton { get; set; } = showCloseButton;
    public string? CloseButtonText { get; set; } = closeButtonText;
    public object? CloseButtonIcon { get; set; } = closeButtonIcon;

    public RenderFragment? MessageTemplate { get; set; } = messageTemplate;

    public Func<SnackbarClosedEventArgs, Task>? OnClosing { get; set; } = onClosing;

    public bool IsHandled { get; set; }
}
