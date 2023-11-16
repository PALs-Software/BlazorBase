export class BlazorBaseAudioRecorder {
    static instances = new Object;

    constructor() {
        this.mediaStream = null;
        this.audioChunks = null;
        this.mediaRecorder = null;
        this.dotNetReference = null;
        this.finalAudioByteArrayBuffer = null;
        this.recordWasCanceled = false;
    }

    static initialize(dotNetReferenceFromServer) {
        BlazorBaseAudioRecorder.instances[dotNetReferenceFromServer._id] = new BlazorBaseAudioRecorder();
        BlazorBaseAudioRecorder.instances[dotNetReferenceFromServer._id].dotNetReference = dotNetReferenceFromServer;
        return dotNetReferenceFromServer._id;
    }

    static dispose(id) {
        delete BlazorBaseAudioRecorder.instances[id]
    }

    static callInstanceFunction(id, functionName, args) {
        if (args === undefined)
            args = [];

        return BlazorBaseAudioRecorder.instances[id][functionName].apply(BlazorBaseAudioRecorder.instances[id], args);
    }

    async startRecord() {
        this.mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        this.mediaRecorder = new MediaRecorder(this.mediaStream, { mimeType: "audio/webm", });

        this.mediaRecorder.addEventListener('dataavailable', e => {
            this.audioChunks.push(e.data);
        });

        this.mediaRecorder.addEventListener('error', e => {
            console.warn('media recorder error: ' + e);
        });

        this.mediaRecorder.addEventListener('stop', async () => {
            if (this.recordWasCanceled)
                return;

            var audioBlob = new Blob(this.audioChunks, { type: "audio/webm" });
            var clientAudioUrl = URL.createObjectURL(audioBlob);
            this.finalAudioByteArrayBuffer = await audioBlob.arrayBuffer();

            this.dotNetReference.invokeMethodAsync('OnRecordFinishedJSInvokable', this.dotNetReference._id, this.finalAudioByteArrayBuffer.byteLength, clientAudioUrl);
        });

        this.recordWasCanceled = false;
        this.audioChunks = [];
        this.mediaRecorder.start();
    }

    pauseRecord() {
        this.mediaRecorder.pause();
    }

    resumeRecord() {
        this.mediaRecorder.resume();
    }

    cancelRecord() {
        this.recordWasCanceled = true;
        this.mediaRecorder.stop();
        this.mediaStream.getTracks().forEach(track => track.stop());
    }

    stopRecord() {
        this.mediaRecorder.stop();
        this.mediaStream.getTracks().forEach(track => track.stop());
    }

    async getRecordBytes(position, length) {
        if (this.finalAudioByteArrayBuffer === null)
            return null;

        return new Uint8Array(this.finalAudioByteArrayBuffer, position, length);
    }
}