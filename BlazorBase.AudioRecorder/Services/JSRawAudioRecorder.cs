using BlazorBase.Modules;
using Microsoft.JSInterop;

namespace BlazorBase.AudioRecorder.Services;

public class JSRawAudioRecorder(IJSRuntime jsRuntime) : JSModul(jsRuntime, "./_content/BlazorBase.AudioRecorder/RawAudioRecorder.js")
{
    #region Properties
    public record OnReceiveDataArgs(long InstanceId, float[] Samples, int SampleRate);
    public event EventHandler<OnReceiveDataArgs>? OnReceiveData;
    #endregion

    #region Members
    protected List<long> InstanceIds = [];
    #endregion

    public async ValueTask<long> InitAsync()
    {
        var instanceId = await InvokeJSAsync<long>("BlazorBaseRawAudioRecorder.initialize", DotNetObjectReference.Create(this));
        InstanceIds.Add(instanceId);
        return instanceId;
    }

    public async ValueTask StartAsync(long instanceId, int? sampleRate = null)
    {
        await InvokeJSVoidAsync("BlazorBaseRawAudioRecorder.callInstanceFunction", instanceId, "startRecord", new object?[] { sampleRate });
    }

    public ValueTask PauseAsync(long instanceId)
    {
        return InvokeJSVoidAsync("BlazorBaseRawAudioRecorder.callInstanceFunction", instanceId, "pauseRecord");
    }

    public ValueTask ResumeAsync(long instanceId)
    {

        return InvokeJSVoidAsync("BlazorBaseRawAudioRecorder.callInstanceFunction", instanceId, "resumeRecord");
    }

    public ValueTask StopAsync(long instanceId)
    {
        return InvokeJSVoidAsync("BlazorBaseRawAudioRecorder.callInstanceFunction", instanceId, "stopRecord");
    }

    [JSInvokable]
    public async Task OnReceiveDataAsync(long instanceId, IJSStreamReference streamReference, int sampleRate)
    {
        using var memoryStream = new MemoryStream();
        using var dataReferenceStream = await streamReference.OpenReadStreamAsync();
        await dataReferenceStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        var floats = new float[bytes.Length / 4];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);

        OnReceiveData?.Invoke(this, new OnReceiveDataArgs(instanceId, floats, sampleRate));
    }

    protected virtual string DateTimeStamp()
    {
        var pOut = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss_ffff");
        return pOut;
    }

    public async ValueTask DisposeInstanceAsync(long instanceId)
    {
        await InvokeJSVoidAsync($"BlazorBaseRawAudioRecorder.dispose", instanceId);
        InstanceIds.Remove(instanceId);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}