class AudioRecorderProcessor extends AudioWorkletProcessor {
    bufferSize = 32 * 1024
    floatsWritten = 0
    buffer = new Float32Array()

    constructor(options) {
        super(options);

        if (options?.parameterData?.bufferSize)
            this.bufferSize = options.parameterData.bufferSize;
        
        this.buffer = new Float32Array(this.bufferSize);
        this.initBuffer();
    }

    initBuffer() {
        this.floatsWritten = 0;
    }

    isBufferEmpty() {
        return this.floatsWritten === 0;
    }

    isBufferFull() {
        return this.floatsWritten === this.bufferSize;
    }

    process(inputs) {
        // use only the 1st channel similar to ScriptProcessorNode
        this.append(inputs[0][0])
        return true;
    }

    append(channelData) {
        if (!channelData)
            return;

        if (this.isBufferFull() || (channelData.length + this.floatsWritten > this.bufferSize))
            this.flush();

        for (let i = 0; i < channelData.length; i++)
            this.buffer[this.floatsWritten++] = channelData[i];
    }

    flush() {
        this.port.postMessage(this.floatsWritten < this.bufferSize ? this.buffer.slice(0, this.floatsWritten) : this.buffer);
        this.initBuffer();
    }

}

registerProcessor("blazor.base.audio.recorder.worklet", AudioRecorderProcessor)