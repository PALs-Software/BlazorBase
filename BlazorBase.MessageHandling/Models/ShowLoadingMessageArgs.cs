using Microsoft.AspNetCore.Components;
using System;

namespace BlazorBase.MessageHandling.Models;

public class ShowLoadingMessageArgs
{
    public ShowLoadingMessageArgs() { }
    public ShowLoadingMessageArgs(string message, RenderFragment? loadingChildContent = null)
    {
        Message = message;
        LoadingChildContent = loadingChildContent;
    }

    public ulong Id { get; set; }
    public string? Message { get; set; }
    public RenderFragment? LoadingChildContent { get; set; }
    public bool IsHandled { get; set; }
}
