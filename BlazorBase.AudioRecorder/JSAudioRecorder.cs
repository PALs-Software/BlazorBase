using BlazorBase.Modules;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace BlazorBase.AudioRecorder;

public class JSAudioRecorder : JSModul
{
    #region Properties

    public record OnNewRecordAvailableArgs(long InstanceId, long AudioByteSize, string AudioBlobUrl);
    public event EventHandler<OnNewRecordAvailableArgs>? OnNewRecordAvailable;

    #endregion

    #region Members
    protected List<long> InstanceIds = new();
    #endregion

    public JSAudioRecorder(IJSRuntime jsRuntime) : base(jsRuntime, "./_content/BlazorBase.AudioRecorder/AudioRecorder.js") { }

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

    public ValueTask StopAsync(long instanceId)
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "stopRecord");
    }

    public ValueTask<byte[]?> GetRecordBytesAsync(long instanceId, long position, long length)
    {
        return InvokeJSAsync<byte[]?>("BlazorBaseAudioRecorder.callInstanceFunction", instanceId, "getRecordBytes", new object[] { position, length });
    }

    [JSInvokable]
    public void OnNewRecordAvailableJSInvokable(long instanceId, long audioByteSize, string audioBlobUrl)
    {
        OnNewRecordAvailable?.Invoke(this, new OnNewRecordAvailableArgs(instanceId, audioByteSize, audioBlobUrl));
    }

    protected virtual string DateTimeStamp()
    {
        var pOut = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss_ffff");
        return pOut;
    }

    public async ValueTask DisposeInstanceAsync(long instanceId)
    {
        await InvokeJSVoidAsync($"BlazorAudioRecorder.dispose", instanceId);
        InstanceIds.Remove(instanceId);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}