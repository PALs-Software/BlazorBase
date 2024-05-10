class AudioRecorderProcessor extends AudioWorkletProcessor {
    bufferSize = 32 * 1024
    bytesWritten = 0
    buffer = new Float32Array(this.bufferSize)

    constructor() {
        super();
        this.initBuffer()
    }

    initBuffer() {
        this.bytesWritten = 0
    }

    isBufferEmpty() {
        return this.bytesWritten === 0
    }

    isBufferFull() {
        return this.bytesWritten === this.bufferSize
    }

    process(inputs) {
        // use only the 1st channel similar to ScriptProcessorNode
        this.append(inputs[0][0])
        return true
    }

    append(channelData) {
        if (this.isBufferFull()) {
            this.flush()
        }

        if (!channelData) return

        for (let i = 0; i < channelData.length; i++) {
            this.buffer[this.bytesWritten++] = channelData[i]
        }
    }

    flush() {
        this.port.postMessage(this.bytesWritten < this.bufferSize ? this.buffer.slice(0, this.bytesWritten) : this.buffer)
        this.initBuffer()
    }

}

registerProcessor("blazor.base.audio.recorder.worklet", AudioRecorderProcessor)