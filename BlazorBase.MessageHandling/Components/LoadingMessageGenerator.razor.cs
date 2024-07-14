using BlazorBase.MessageHandling.Interfaces;
using BlazorBase.MessageHandling.Models;
using BlazorBase.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBase.MessageHandling.Components;

public partial class LoadingMessageGenerator
{
    #region Injects
    [Inject] protected BaseErrorHandler ErrorHandler { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<MessageGenerator> Localizer { get; set; } = null!;
    #endregion

    #region Members
    protected SortedDictionary<ulong, ShowLoadingMessageArgs> LoadingMessages = [];
    protected ulong LastUsedId = 0;
    protected bool Visible = false;
    private ReaderWriterLockSlim Lock = new();
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await InvokeAsync(() =>
        {
            MessageHandler.OnShowLoadingMessage += MessageHandler_OnShowLoadingMessage;
            MessageHandler.OnUpdateLoadingMessage += MessageHandler_OnUpdateLoadingMessage;
            MessageHandler.OnCloseLoadingMessage += MessageHandler_OnCloseLoadingMessage;
            MessageHandler.OnShowLoadingProgressMessage += MessageHandler_OnShowLoadingProgressMessage;
            MessageHandler.OnUpdateLoadingProgressMessage += MessageHandler_OnUpdateLoadingProgressMessage;
            MessageHandler.OnCloseLoadingProgressMessage += MessageHandler_OnCloseLoadingProgressMessage;
        });
    }
    #endregion

    #region Message Handler
    protected ulong MessageHandler_OnShowLoadingMessage(ShowLoadingMessageArgs args)
    {
        return ShowLoadingMessage(args);
    }

    private bool MessageHandler_OnUpdateLoadingMessage(ShowLoadingMessageArgs args)
    {
        return UpdateLoadingMessage(args);
    }

    private bool MessageHandler_OnCloseLoadingMessage(ulong id)
    {
        return CloseLoadingMessage(id);
    }

    private ulong MessageHandler_OnShowLoadingProgressMessage(ShowLoadingProgressMessageArgs args)
    {
        return ShowLoadingMessage(args);
    }
    private bool MessageHandler_OnUpdateLoadingProgressMessage(ShowLoadingProgressMessageArgs args)
    {
        return UpdateLoadingMessage(args);
    }

    private bool MessageHandler_OnCloseLoadingProgressMessage(ulong id)
    {
        return CloseLoadingMessage(id);
    }
    #endregion

    #region Methods

    #region Show
    protected ulong ShowLoadingMessage(ShowLoadingMessageArgs args)
    {
        if (args.IsHandled)
            return args.Id;

        args.IsHandled = true;
        Lock.EnterWriteLock();
        var id = LastUsedId;
        if (LoadingMessages.Count == 0)
            id = 0;
        id++;

        LastUsedId = id;
        args.Id = id;
        LoadingMessages.Add(id, args);
        Lock.ExitWriteLock();

        if (args is ShowLoadingProgressMessageArgs progressArgs)
            progressArgs.AbortButtonText ??= Localizer["Abort"];

        Visible = LoadingMessages.Count > 0;
        InvokeAsync(StateHasChanged);

        return args.Id;
    }

    public void ShowLoadingMessage(string message, RenderFragment? loadingChildContent = null)
    {
        ShowLoadingMessage(new ShowLoadingMessageArgs(message, loadingChildContent));
    }

    public void ShowLoadingProgressMessage(string message,
                                           int currentProgress = 0,
                                           string? progressText = null,
                                           bool showProgressInText = true,
                                           RenderFragment? loadingChildContent = null)
    {
        ShowLoadingMessage(new ShowLoadingProgressMessageArgs(message, currentProgress, progressText, showProgressInText, loadingChildContent));
    }
    #endregion

    #region Update
    protected bool UpdateLoadingMessage(ShowLoadingMessageArgs args)
    {
        Lock.EnterReadLock();
        var success = LoadingMessages.TryGetValue(args.Id, out ShowLoadingMessageArgs? loadingMessage);
        Lock.ExitReadLock();
        if (!success || loadingMessage == null)
            return false;

        loadingMessage.Message = args.Message;
        loadingMessage.LoadingChildContent = args.LoadingChildContent;

        if (loadingMessage is not ShowLoadingProgressMessageArgs loadingProgressArgs || args is not ShowLoadingProgressMessageArgs progressArgs)
            return true;

        loadingProgressArgs.ProgressText = progressArgs.ProgressText;
        loadingProgressArgs.CurrentProgress = progressArgs.CurrentProgress;
        loadingProgressArgs.ShowProgressInText = progressArgs.ShowProgressInText;

        InvokeAsync(StateHasChanged);
        return true;
    }

    public void UpdateLoadingMessage(ulong id,
                                     string message,
                                     RenderFragment? loadingChildContent = null)
    {
        UpdateLoadingMessage(new ShowLoadingMessageArgs(message, loadingChildContent) { Id = id });
    }

    public void UpdateLoadingProgressMessage(ulong id,
                                             string message,
                                             int currentProgress = 0,
                                             string? progressText = null,
                                             bool showProgressInText = true,
                                             RenderFragment? loadingChildContent = null)
    {
        ShowLoadingMessage(new ShowLoadingProgressMessageArgs(message, currentProgress, progressText, showProgressInText, loadingChildContent) { Id = id });
    }
    #endregion

    #region Close
    public bool CloseLoadingMessage(ulong id)
    {
        Lock.EnterWriteLock();
        var success = LoadingMessages.Remove(id, out var _);
        Lock.ExitWriteLock();

        Visible = LoadingMessages.Count > 0;
        InvokeAsync(StateHasChanged);
        return success;
    }

    public void CloseLoadingProgressMessage(ulong id)
    {
        CloseLoadingMessage(id);
    }
    #endregion

    #region Abort
    protected Task OnAbortButtonClickedAsync(ulong id, ShowLoadingProgressMessageArgs progressArgs)
    {
        return Task.Run(() =>
            progressArgs.OnAborting?.Invoke(id)
        );
    }
    #endregion

    #endregion
}
