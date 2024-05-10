using BlazorBase.Modules;
using Microsoft.JSInterop;

namespace BlazorBase.AudioRecorder.Services;

public class JSAudioRecorder(IJSRuntime jsRuntime) : JSModul(jsRuntime, "./_content/BlazorBase.AudioRecorder/AudioRecorder.js")
{
    #region Properties
    public record OnRecordFinishedArgs(long InstanceId, long AudioByteSize, string ClientAudioBlobUrl);
    public event EventHandler<OnRecordFinishedArgs>? OnRecordFinished;
    #endregion

    #region Members
    protected List<long> InstanceIds = [];
    #endregion

    public async ValueTask<long> InitAsync()
    {
        var instanceId = await InvokeJSAsync<long>("BlazorBaseAudioRecorder.initialize", DotNetObjectReference.Create(this));
        InstanceIds.Add(instanceId);
        return instanceId;
    }

    public async ValueTask StartAsync(long instanceId)
    {
        await InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "startRecord");
    }

    public ValueTask PauseAsync(long instanceId)
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "pauseRecord");
    }

    public ValueTask ResumeAsync(long instanceId)
    {

        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "resumeRecord");
    }

    public ValueTask CancelAsync(long instanceId)
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "cancelRecord");
    }

    public ValueTask StopAsync(long instanceId)
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "stopRecord");
    }

    public ValueTask<byte[]?> GetRecordBytesAsync(long instanceId, long position, long length)
    {
        return InvokeJSAsync<byte[]?>("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "getRecordBytes", new object[] { position, length });
    }

    [JSInvokable]
    public void OnRecordFinishedJSInvokable(long instanceId, long audioByteSize, string clientAudioBlobUrl)
    {
        OnRecordFinished?.Invoke(this, new OnRecordFinishedArgs(instanceId, audioByteSize, clientAudioBlobUrl));
    }

    protected virtual string DateTimeStamp()
    {
        var pOut = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss_ffff");
        return pOut;
    }

    public async ValueTask DisposeInstanceAsync(long instanceId)
    {
        await InvokeJSVoidAsync($"BlazorBaseAudioRecorder.dispose", instanceId);
        InstanceIds.Remove(instanceId);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}