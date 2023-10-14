using Microsoft.AspNetCore.Components;

namespace BlazorBase.AudioRecorder;

public partial class AudioRecorder
{

    #region Injects

    [Inject] protected JSAudioRecorder JSAudioRecorder { get; set; } = null!;

    #endregion



    #region Members

    string mUrl;
    bool mDisableRecordAudioStart;
    bool mDisableRecordAudioPause = true;
    bool mDisableRecordAudioResume = true;
    bool mDisableRecordAudioStop = true;
    bool mDisableDownloadBlob = true;

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        JSAudioRecorder.OnNewAudioUrlReceived += JSAudioRecorder_OnNewAudioUrlReceived;
    }

    private void JSAudioRecorder_OnNewAudioUrlReceived(object? sender, string newAudioUrl)
    {
        mUrl = newAudioUrl;
    }

    void butRecordAudioStart_Click()
    {
        mUrl = "";
        mDisableRecordAudioStart = true;
        mDisableRecordAudioPause = false;
        mDisableRecordAudioResume = true;
        mDisableRecordAudioStop = false;
        mDisableDownloadBlob = true;
        _ = JSAudioRecorder.StartAsync();
    }

    void butRecordAudioPause_Click()
    {
        mDisableRecordAudioStart = true;
        mDisableRecordAudioPause = true;
        mDisableRecordAudioResume = false;
        mDisableRecordAudioStop = false;
        mDisableDownloadBlob = true;
        JSAudioRecorder.PauseAsync();
    }

    void butRecordAudioResume_Click()
    {
        mDisableRecordAudioStart = true;
        mDisableRecordAudioPause = false;
        mDisableRecordAudioResume = true;
        mDisableRecordAudioStop = false;
        mDisableDownloadBlob = true;
        JSAudioRecorder.ResumeAsync();
    }

    void butRecordAudioStop_Click()
    {
        mDisableRecordAudioStart = false;
        mDisableRecordAudioPause = true;
        mDisableRecordAudioResume = true;
        mDisableRecordAudioStop = true;
        mDisableDownloadBlob = false;
        JSAudioRecorder.StopAsync();
    }

    void butDownloadBlob_Click()
    {
        JSAudioRecorder.DownloadBlobAsync(mUrl);
    }
}
