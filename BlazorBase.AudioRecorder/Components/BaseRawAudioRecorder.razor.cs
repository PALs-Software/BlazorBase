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
    [Parameter] public int? BufferSize { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool ShowTimer { get; set; } = true;
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
        if (!ShowTimer)
            Timer.Stop();

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

    #region Audio Handling

    public async Task StartAudioRecordAsync()
    {
        AudioRecordState = BaseAudioRecordState.Recording;

        JSRawAudioRecorderId ??= await JSRawAudioRecorder.InitAsync();
        await JSRawAudioRecorder.StartAsync(JSRawAudioRecorderId.Value, SampleRate, BufferSize);
        StartTimer();

        await OnStartAudio.InvokeAsync();
    }

    public async Task PauseAudioRecordAsync()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Paused;

        await JSRawAudioRecorder.PauseAsync(JSRawAudioRecorderId.Value);
        StopTimer();

        await OnPauseAudio.InvokeAsync();
    }

    public async Task ResumeAudioRecordAsync()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Recording;

        await JSRawAudioRecorder.ResumeAsync(JSRawAudioRecorderId.Value);
        StopTimer();

        await OnResumeAudio.InvokeAsync();
    }

    public async Task CancelAudioRecordAsync()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Stopped;

        await JSRawAudioRecorder.StopAsync(JSRawAudioRecorderId.Value);
        StopTimer();

        await OnCancelAudio.InvokeAsync();
    }

    public async Task StopAudioRecordAsync()
    {
        if (JSRawAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Stopped;

        await JSRawAudioRecorder.StopAsync(JSRawAudioRecorderId.Value);
        StopTimer();

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

    protected void StartTimer()
    {
        if (!ShowTimer)
            return;
        Stopwatch.Restart();
        Timer.Start();
    }

    protected void StopTimer()
    {
        if (!ShowTimer)
            return;
        Stopwatch.Stop();
        Timer.Stop();
    }
    #endregion
}
