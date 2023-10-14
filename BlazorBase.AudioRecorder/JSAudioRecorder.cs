using BlazorBase.Modules;
using Microsoft.JSInterop;

namespace BlazorBase.AudioRecorder;

public class JSAudioRecorder : JSModul
{
    #region Properties

    public event EventHandler<string>? OnNewAudioUrlReceived;

    #endregion

    #region Member
    private long? Id = null;
    #endregion

    public JSAudioRecorder(IJSRuntime jsRuntime) : base(jsRuntime, "./_content/BlazorBase.AudioRecorder/AudioRecorder.js") { }

    public async ValueTask StartAsync()
    {
        try
        {
            Id ??= await InvokeJSAsync<long>("BlazorBaseAudioRecorder.initialize", DotNetObjectReference.Create(this));
            await InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", Id, "startRecord");
        }
        catch (Exception e)
        {
            var ex = e.Message;
        }
        
    }

    public ValueTask PauseAsync()
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", Id!, "pauseRecord");
    }

    public ValueTask ResumeAsync()
    {

        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", Id!, "resumeRecord");
    }

    public ValueTask StopAsync()
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", Id!, "stopRecord");
    }

    public ValueTask DownloadBlobAsync(string audioUrl)
    {
        return InvokeJSVoidAsync("BlazorBaseAudioRecorder.callInstanceFunction", Id!, "downloadBlob", new object[] { audioUrl, "MyRecording_" + DateTimeStamp() + ".mp3" });
    }

    [JSInvokable]
    public void OnNewAudioUrlCreated(string newAudioUrl)
    {
        OnNewAudioUrlReceived?.Invoke(this, newAudioUrl);
    }

    protected virtual string DateTimeStamp()
    {
        var pOut = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss_ffff");
        return pOut;
    }

    public override async ValueTask DisposeAsync()
    {
        if (Id.HasValue)
            await InvokeJSVoidAsync($"BlazorAudioRecorder.dispose", Id);

        await base.DisposeAsync();
    }
}