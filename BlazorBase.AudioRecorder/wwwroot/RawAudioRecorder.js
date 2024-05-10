export class BlazorBaseRawAudioRecorder {
    static instances = new Object;

    constructor() {
        this.userMedia = null;
        this.mediaStreamSource = null;
        this.audioChunks = null;
        this.audioContext = null;
        this.audioRecorder = null;
        this.dotNetReference = null;
        this.finalAudioByteArrayBuffer = null;
    }

    static initialize(dotNetReferenceFromServer) {
        BlazorBaseRawAudioRecorder.instances[dotNetReferenceFromServer._id] = new BlazorBaseRawAudioRecorder();
        BlazorBaseRawAudioRecorder.instances[dotNetReferenceFromServer._id].dotNetReference = dotNetReferenceFromServer;
        return dotNetReferenceFromServer._id;
    }

    static dispose(id) {
        delete BlazorBaseRawAudioRecorder.instances[id]
    }

    static callInstanceFunction(id, functionName, args) {
        if (args === undefined)
            args = [];

        return BlazorBaseRawAudioRecorder.instances[id][functionName].apply(BlazorBaseRawAudioRecorder.instances[id], args);
    }

    async startRecord(sampleRate = null) {
        this.userMedia = await navigator.mediaDevices.getUserMedia({ audio: true });
        var options = {}
        if (sampleRate !== null) {
            options = { sampleRate: sampleRate };
        }

        this.audioContext = new AudioContext(options);
        this.mediaStreamSource = this.audioContext.createMediaStreamSource(this.userMedia);

        await this.audioContext.audioWorklet.addModule("_content/BlazorBase.AudioRecorder/AudioRecorderProcessor.js")
        this.audioRecorder = new AudioWorkletNode(this.audioContext, "blazor.base.audio.recorder.worklet")

        this.mediaStreamSource
            .connect(this.audioRecorder)
            .connect(this.audioContext.destination);

        this.audioChunks = [];
        this.audioRecorder.port.onmessage = async (e) => {            
            var streamReference = DotNet.createJSStreamReference(e.data);
            await this.dotNetReference.invokeMethodAsync('OnReceiveDataAsync', this.dotNetReference._id, streamReference, this.audioContext.sampleRate);
            DotNet.disposeJSObjectReference(streamReference);
        }
    }

    async pauseRecord() {
        await this.audioContext.suspend();
    }

    async resumeRecord() {
        await this.audioContext.resume();
    }

    async stopRecord() {
        this.userMedia.getTracks().forEach(track => track.stop());
        this.audioRecorder.disconnect(this.audioContext.destination);
        this.mediaStreamSource.disconnect(this.audioRecorder);
        await this.audioContext.close();
    }

    async getRecordBytes(position, length) {
        if (this.finalAudioByteArrayBuffer === null)
            return null;

        return new Uint8Array(this.finalAudioByteArrayBuffer, position, length);
    }
}