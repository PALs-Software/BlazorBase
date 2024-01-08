using BlazorBase.AudioRecorder.Enums;
using BlazorBase.AudioRecorder.Services;
using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace BlazorBase.AudioRecorder.Components;

public partial class BaseAudioRecorder : IAsyncDisposable
{
    #region Injects
    [Inject] protected JSAudioRecorder JSAudioRecorder { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<BaseAudioRecorder> Localizer { get; set; } = null!;
    #endregion

    #region Parameters

    [Parameter] public int MaxAudioChunkTransferSizeInBytes { get; set; } = 32 * 1024;
    [Parameter] public bool ShowAudioResultDirectly { get; set; }
    [Parameter] public string? Class { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    public record UploadProcessedArgs(double Progress, double Percentage);
    [Parameter] public EventCallback<UploadProcessedArgs> OnUploadProgressed { get; set; }
    [Parameter] public EventCallback<JSAudioRecorder.OnRecordFinishedArgs> OnRecordFinished { get; set; }

    #endregion

    #region Properties
    public BaseAudioRecordState AudioRecordState { get; protected set; } = BaseAudioRecordState.Stopped;
    #endregion

    #region Members

    protected long? JSAudioRecorderId = null;
    protected string ClientAudioBlobUrl = String.Empty;

    protected bool ShowLoadingIndicator = false;
    protected UploadProcessedArgs UploadProgress = new(0, 0);
    protected CancellationTokenSource UploadAudioDataCancellationTokenSource = new();

    protected System.Timers.Timer Timer = new(1000);
    protected Stopwatch Stopwatch = new();

    #endregion

    #region Init

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        JSAudioRecorder.OnRecordFinished += JSAudioRecorder_OnNewRecordAvailable;

        Timer.Elapsed += TimerElapsed;
        Timer.AutoReset = true;
    }

    private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    #endregion

    #region UI Events

    protected async Task StartAudioRecord()
    {
        ClientAudioBlobUrl = String.Empty;
        AudioRecordState = BaseAudioRecordState.Recording;

        JSAudioRecorderId ??= await JSAudioRecorder.InitAsync();
        await JSAudioRecorder.StartAsync(JSAudioRecorderId.Value);
        Stopwatch.Restart();
        Timer.Start();
    }

    protected async Task PauseAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Paused;

        await JSAudioRecorder.PauseAsync(JSAudioRecorderId.Value);
        Stopwatch.Stop();
        Timer.Stop();
    }

    protected async Task ResumeAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Recording;

        await JSAudioRecorder.ResumeAsync(JSAudioRecorderId.Value);
        Stopwatch.Start();
        Timer.Start();
    }

    protected async Task CancelAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Stopped;

        await JSAudioRecorder.CancelAsync(JSAudioRecorderId.Value);
        Stopwatch.Stop();
        Timer.Stop();
    }

    protected async Task StopAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        AudioRecordState = BaseAudioRecordState.Stopped;

        await JSAudioRecorder.StopAsync(JSAudioRecorderId.Value);
        Stopwatch.Stop();
        Timer.Stop();
    }

    #endregion

    #region Audio Record Data Transfer

    private void JSAudioRecorder_OnNewRecordAvailable(object? sender, JSAudioRecorder.OnRecordFinishedArgs args)
    {
        if (JSAudioRecorderId == null || JSAudioRecorderId != args.InstanceId)
            return;

        OnRecordFinished.InvokeAsync(args);
        ClientAudioBlobUrl = args.ClientAudioBlobUrl;
        InvokeAsync(StateHasChanged);
    }




    public async Task<bool> GetRecordedAudioDataAsync(long totalAudioByteSize, Stream stream, CancellationToken cancellationToken)
    {
        if (JSAudioRecorderId == null)
            return false;

        try
        {
            long position = 0;
            ShowLoadingIndicator = true;

            while (position < totalAudioByteSize)
            {
                UploadAudioDataCancellationTokenSource.Token.ThrowIfCancellationRequested();
                cancellationToken.ThrowIfCancellationRequested();
                var length = Math.Min(MaxAudioChunkTransferSizeInBytes, totalAudioByteSize - position);

                var buffer = await JSAudioRecorder.GetRecordBytesAsync(JSAudioRecorderId.Value, position, length);
                if (buffer == null)
                    throw new InvalidOperationException($"Transferd byte array is null");

                if (length != buffer.Length)
                    throw new InvalidOperationException($"Requested a maximum of {length}, but received {buffer.Length}");

                await stream.WriteAsync(buffer, cancellationToken);

                position += buffer.Length;
                var progress = (double)position / totalAudioByteSize;
                UploadProgress = new(progress, progress * 100);
                await OnUploadProgressed.InvokeAsync(UploadProgress);
            }
        }
        catch (Exception e)
        {
            MessageHandler.ShowMessage(Localizer["Error by uploading audio record to the server"], Localizer["An error occured by uploading the audio record to the server: {0}", e.Message], MessageType.Error);
            return false;
        }
        finally
        {
            ShowLoadingIndicator = false;
            _ = InvokeAsync(StateHasChanged);
        }

        return true;
    }

    #endregion

    #region MISC
    public ValueTask DisposeAsync()
    {
        if (JSAudioRecorderId == null)
            return ValueTask.CompletedTask;

        UploadAudioDataCancellationTokenSource.Cancel();

        return JSAudioRecorder.DisposeInstanceAsync(JSAudioRecorderId.Value);
    }
    #endregion
}
