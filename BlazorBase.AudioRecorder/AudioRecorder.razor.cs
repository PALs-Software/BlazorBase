using BlazorBase.MessageHandling.Enum;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBase.AudioRecorder;

public partial class AudioRecorder : IAsyncDisposable
{
    #region Injects
    [Inject] protected JSAudioRecorder JSAudioRecorder { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    [Inject] protected IStringLocalizer<AudioRecorder> Localizer { get; set; } = null!;
    #endregion

    #region Properties
    [Parameter] public int MaxAudioChunkTransferSizeInBytes { get; set; } = 32 * 1024;

    public record UploadProcessedArgs(double Progress, double Percentage);
    [Parameter] public EventCallback<UploadProcessedArgs> OnUploadProgressed { get; set; }
    [Parameter] public EventCallback<JSAudioRecorder.OnNewRecordAvailableArgs> OnNewRecordAvailable { get; set; }
    #endregion

    #region Members

    protected long? JSAudioRecorderId = null;
    protected string AudioBlobUrl = String.Empty;
    protected bool DisableRecordAudioStart;
    protected bool DisableRecordAudioPause = true;
    protected bool DisableRecordAudioResume = true;
    protected bool DisableRecordAudioStop = true;

    protected bool ShowLoadingIndicator = false;
    protected UploadProcessedArgs UploadProgress = new(0, 0);
    protected CancellationTokenSource UploadAudioDataCancellationTokenSource = new();

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        JSAudioRecorder.OnNewRecordAvailable += JSAudioRecorder_OnNewRecordAvailable;
    }

    private void JSAudioRecorder_OnNewRecordAvailable(object? sender, JSAudioRecorder.OnNewRecordAvailableArgs args)
    {
        if (JSAudioRecorderId == null || JSAudioRecorderId != args.InstanceId)
            return;

        OnNewRecordAvailable.InvokeAsync(args);
        AudioBlobUrl = args.AudioBlobUrl;
        _ = SaveAudioRecordAsync(args.AudioByteSize);

        InvokeAsync(StateHasChanged);
    }

    protected async Task SaveAudioRecordAsync(long totalAudioByteSize)
    {
        using var memoryStream = new MemoryStream();
        await WriteAudioDataToStreamAsync(totalAudioByteSize, memoryStream, UploadAudioDataCancellationTokenSource.Token);
        var bytes = memoryStream.ToArray();
    }

    protected async Task WriteAudioDataToStreamAsync(long totalAudioByteSize, Stream stream, CancellationToken cancellationToken)
    {
        if (JSAudioRecorderId == null)
            return;

        try
        {
            long position = 0;
            ShowLoadingIndicator = true;

            while (position < totalAudioByteSize)
            {
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
        }
        finally
        {
            ShowLoadingIndicator = false;
            _ = InvokeAsync(StateHasChanged);
        }
    }

    protected async Task StartAudioRecord()
    {
        AudioBlobUrl = String.Empty;
        DisableRecordAudioStart = true;
        DisableRecordAudioPause = false;
        DisableRecordAudioResume = true;
        DisableRecordAudioStop = false;

        JSAudioRecorderId ??= await JSAudioRecorder.InitAsync();
        await JSAudioRecorder.StartAsync(JSAudioRecorderId.Value);
    }

    protected async Task PauseAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        DisableRecordAudioStart = true;
        DisableRecordAudioPause = true;
        DisableRecordAudioResume = false;
        DisableRecordAudioStop = false;

        await JSAudioRecorder.PauseAsync(JSAudioRecorderId.Value);
    }

    protected async Task ResumeAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        DisableRecordAudioStart = true;
        DisableRecordAudioPause = false;
        DisableRecordAudioResume = true;
        DisableRecordAudioStop = false;

        await JSAudioRecorder.ResumeAsync(JSAudioRecorderId.Value);
    }

    protected async Task StopAudioRecord()
    {
        if (JSAudioRecorderId == null)
            return;

        DisableRecordAudioStart = false;
        DisableRecordAudioPause = true;
        DisableRecordAudioResume = true;
        DisableRecordAudioStop = true;

        await JSAudioRecorder.StopAsync(JSAudioRecorderId.Value);
    }

    public ValueTask DisposeAsync()
    {
        if (JSAudioRecorderId == null)
            return ValueTask.CompletedTask;

        UploadAudioDataCancellationTokenSource.Cancel();

        return JSAudioRecorder.DisposeInstanceAsync(JSAudioRecorderId.Value);
    }
}
