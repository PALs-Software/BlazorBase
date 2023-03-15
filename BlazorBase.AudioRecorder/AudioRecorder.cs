using BlazorBase.Modules;
using Microsoft.JSInterop;

namespace BlazorBase.AudioRecorder
{
    public class AudioRecorder : JSModul
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public AudioRecorder(IJSRuntime jsRuntime) : base(jsRuntime, "./_content/BlazorBase.AudioRecorder/AudioRecorder.js")
        {
        }

        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("showPrompt", message);
        }
    }
}