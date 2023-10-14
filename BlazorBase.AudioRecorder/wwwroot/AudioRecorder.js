export class BlazorBaseAudioRecorder {
    static instances = new Object;

    constructor() {
        this.mediaStream = null;
        this.audioChunks = null;
        this.mediaRecorder = null;
        this.dotNetReference = null;
    }

    static initialize(dotNetReferenceFromServer) {
        console.log("Init", dotNetReferenceFromServer._id);


        BlazorBaseAudioRecorder.instances[dotNetReferenceFromServer._id] = new BlazorBaseAudioRecorder();
        BlazorBaseAudioRecorder.instances[dotNetReferenceFromServer._id].dotNetReference = dotNetReferenceFromServer;

        console.log("instances", BlazorBaseAudioRecorder.instances);

        return dotNetReferenceFromServer._id;
    }

    static dispose(id) {
        console.log("dispose", id);
        delete BlazorBaseAudioRecorder.instances[id]
    }

    static callInstanceFunction(id, functionName, args) {
        console.log("call", id, functionName, args);
        if (args === undefined)
            args = [];

        return BlazorBaseAudioRecorder.instances[id][functionName].apply(BlazorBaseAudioRecorder.instances[id], args);
    }

    async startRecord() {
        console.log("start", this.dotNetReference._id);
        this.mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        this.mediaRecorder = new MediaRecorder(this.mediaStream);

        this.mediaRecorder.addEventListener('dataavailable', vEvent => {
            this.audioChunks.push(vEvent.data);
        });

        this.mediaRecorder.addEventListener('error', vError => {
            console.warn('media recorder error: ' + vError);
        });

        this.mediaRecorder.addEventListener('stop', () => {
            var audioBlob = new Blob(this.audioChunks, { type: "audio/mp3;" });
            var audioUrl = URL.createObjectURL(audioBlob);
            this.dotNetReference.invokeMethodAsync('OnNewAudioUrlCreated', audioUrl);
        });

        this.audioChunks = [];
        this.mediaRecorder.start();
    }

    pauseRecord() {
        this.mediaRecorder.pause();
    }

    resumeRecord() {
        this.mediaRecorder.resume();
    }

    stopRecord() {
        this.mediaRecorder.stop();
        this.mediaStream.getTracks().forEach(pTrack => pTrack.stop());
    }

    downloadBlob(url, name) {
        // Create a link element
        const link = document.createElement("a");
        // Set the link's href to point to the Blob URL
        link.href = url;
        link.download = name;
        // Append link to the body
        document.body.appendChild(link);
        // Dispatch click event on the link
        // This is necessary as link.click() does not work on the latest firefox
        link.dispatchEvent(
            new MouseEvent('click', {
                bubbles: true,
                cancelable: true,
                view: window
            })
        );
        // Remove the link from the body
        document.body.removeChild(link);
    }

}

//export { BlazorBaseAudioRecorder }

/*
var BlazorAudioRecorder = {};
console.log("improted");
console.log(window.blazorBase);

window.blazorBase.audioRecorder = {

    init: function (vCaller) {
        console.log("init2");       
    }
};

let blazorBase = window.blazorBase
export { blazorBase }

(function () {
    var mStream;
    var mAudioChunks;
    var mMediaRecorder;
    var mCaller;

    BlazorAudioRecorder.Initialize = function (vCaller) {
        console.log("init");
        mCaller = vCaller;
    };

    BlazorAudioRecorder.StartRecord = async function () {
        mStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        mMediaRecorder = new MediaRecorder(mStream);
        mMediaRecorder.addEventListener('dataavailable', vEvent => {
            mAudioChunks.push(vEvent.data);
        });

        mMediaRecorder.addEventListener('error', vError => {
            console.warn('media recorder error: ' + vError);
        });

        mMediaRecorder.addEventListener('stop', () => {
            var pAudioBlob = new Blob(mAudioChunks, { type: "audio/mp3;" });
            var pAudioUrl = URL.createObjectURL(pAudioBlob);
            mCaller.invokeMethodAsync('OnAudioUrl', pAudioUrl);

            // uncomment the following if you want to play the recorded audio (without the using the audio HTML element)
            //var pAudio = new Audio(pAudioUrl);
            //pAudio.play();
        });

        console.log("startt");
        mAudioChunks = [];
        mMediaRecorder.start();
        console.log("test");
    };

    BlazorAudioRecorder.PauseRecord = function () {
        mMediaRecorder.pause();
    };

    BlazorAudioRecorder.ResumeRecord = function () {
        mMediaRecorder.resume();
    };

    BlazorAudioRecorder.StopRecord = function () {
        mMediaRecorder.stop();
        mStream.getTracks().forEach(pTrack => pTrack.stop());
    };

    BlazorAudioRecorder.DownloadBlob = function (vUrl, vName) {
        // Create a link element
        const link = document.createElement("a");

        // Set the link's href to point to the Blob URL
        link.href = vUrl;
        link.download = vName;

        // Append link to the body
        document.body.appendChild(link);

        // Dispatch click event on the link
        // This is necessary as link.click() does not work on the latest firefox
        link.dispatchEvent(
            new MouseEvent('click', {
                bubbles: true,
                cancelable: true,
                view: window
            })
        );

        // Remove the link from the body
        document.body.removeChild(link);
    };
})();
*/