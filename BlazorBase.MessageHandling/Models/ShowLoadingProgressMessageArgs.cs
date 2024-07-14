using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Models;

public class ShowLoadingProgressMessageArgs : ShowLoadingMessageArgs
{
    public ShowLoadingProgressMessageArgs() { }
    public ShowLoadingProgressMessageArgs(string message,
                                          int currentProgress = 0,
                                          string? progressText = null,                                              
                                          bool showProgressInText = true,
                                          RenderFragment? loadingChildContent = null,
                                          string? abortButtonText = null,
                                          Func<ulong, Task>? onAborting = null) : base(message, loadingChildContent)
    {
        ProgressText = progressText;
        CurrentProgress = currentProgress;
        ShowProgressInText = showProgressInText;
        AbortButtonText = abortButtonText;
        OnAborting = onAborting;
    }

    public string? ProgressText { get; set; }
    public int CurrentProgress { get; set; }
    public bool ShowProgressInText { get; set; }
    public string? AbortButtonText { get; set; }
    public Func<ulong, Task>? OnAborting { get; set; }
}
