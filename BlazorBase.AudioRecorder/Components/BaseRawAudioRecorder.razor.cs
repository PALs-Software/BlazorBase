using BlazorBase.AudioRecorder.Enums;
using BlazorBase.AudioRecorder.Services;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace BlazorBase.AudioRecorder.Components;

public partial class BaseRawAudioRecorder : IAsyncDisposable
{
    #region Injects
    [Inject] protected JSRawAudioRecorder JSRawAudioRecorder { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<BaseAudioRecorder> Localizer { get; set; } = null!;
    #endregion

    #region Parameters
    [Parameter] public int? SampleRate { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback OnStartAudio { get; set; }
    [Parameter] public EventCallback OnPauseAudio { get; set; }
    [Parameter] public EventCallback OnResumeAudio { get; set; }
    [Parameter] public EventCallback OnCancelAudio { get; set; }
    [Parameter] public EventCallback OnStopAudio { get; set; }
    [Parameter] public EventCallback<JSRawAudioRecorder.OnReceiveDataArgs> OnReceiveData { get; set; }
    #endregion

    #region Properties
    public BaseAudioRecordState AudioRecordState { get; protected set; } = BaseAudioRecordState.Stopped;
    #endregion

    #region Members
    protected long? JSRawAudioRecorderId = null;

    protected System.Timers.Timer Timer = new(1000);
    protected Stopwatch Stopwatch = new();
    #endregion

    #region Init

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        JSRawAudioRecorder.OnReceiveData += JSRawAudioRecorder_OnReceiveData;

        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = true;
    }

    private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Audio Record Data Transfer
    private void JSRawAudioRecorder_OnReceiveData(object? sender, JSRawAudioRecorder.OnReceiveDataArgs args)
    {
        if (JSRawAudioRecorderId == null || JSRawAudioRecorderId != args.InstanceId)
            return;

        OnReceiveData.InvokeAsync(args);
    }
    #endregion

    #region UI Events

    protected async Task StartAudioRecord()
    {
        AudioRecordState = BaseAudioRecordState.Recording;

        JSRawAudioRecorderId ??= await JSRawAudioRecorder.InitAsync();
        await JSRawAudioRecorder.StartAsync(JSRawAudioRecorderId.Value, SampleRate);
        Stopwatch.Restart();
        Timer.Start();

        await OnStartAudio.InvokeAsync();
    }

    protected async Task PauseAudioRecord()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Paused;

        await JSRawAudioRecorder.PauseAsync(JSRawAudioRecorderId.Value);
        Stopwatch.Stop();
        Timer.Stop();

        await OnPauseAudio.InvokeAsync();
    }

    protected async Task ResumeAudioRecord()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Recording;

        await JSRawAudioRecorder.ResumeAsync(JSRawAudioRecorderId.Value);
        Stopwatch.Start();
        Timer.Start();

        await OnResumeAudio.InvokeAsync();
    }

    protected async Task CancelAudioRecord()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Stopped;

        await JSRawAudioRecorder.StopAsync(JSRawAudioRecorderId.Value);
        Stopwatch.Stop();
        Timer.Stop();

        await OnCancelAudio.InvokeAsync();
    }

    protected async Task StopAudioRecord()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Stopped;

        await JSRawAudioRecorder.StopAsync(JSRawAudioRecorderId.Value);
        Stopwatch.Stop();
        Timer.Stop();

        await OnStopAudio.InvokeAsync();
    }

    #endregion

    #region MISC
    public ValueTask DisposeAsync()
    {
        if (JSRawAudioRecorderId == null)
            return ValueTask.CompletedTask;

        return JSRawAudioRecorder.DisposeInstanceAsync(JSRawAudioRecorderId.Value);
    }
    #endregion
}
