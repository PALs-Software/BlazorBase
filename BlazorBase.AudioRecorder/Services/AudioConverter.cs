namespace BlazorBase.AudioRecorder.Services;

public class AudioConverter
{
    public short[] ConvertFloatToShortSamples(float[] floatSamples)
    {
        var shortSamples = new short[floatSamples.Length];
        for (int i = 0; i < floatSamples.Length; i++)
            shortSamples[i] = (short)(floatSamples[i] * 32767); // Convert float (-1 to 1) to int16 (−2^15 and 2^15−1)

        return shortSamples;
    }

    public byte[] ConvertSamplesToWav(short[] samples, int samplesPerSecond = 8000, short bitsPerSample = 16)
    {
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);

        int formatChunkSize = 16;
        int headerSize = 8;
        short formatType = 1;
        short tracks = 1;
        short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
        int bytesPerSecond = samplesPerSecond * frameSize;
        int waveSize = 4;
        int dataChunkSize = samples.Length * frameSize;
        int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

        binaryWriter.Write(0x46464952); // RIFF
        binaryWriter.Write(fileSize);
        binaryWriter.Write(0x45564157); // WAVE
        binaryWriter.Write(0x20746D66); // fmt 
        binaryWriter.Write(formatChunkSize);
        binaryWriter.Write(formatType);
        binaryWriter.Write(tracks);
        binaryWriter.Write(samplesPerSecond);
        binaryWriter.Write(bytesPerSecond);
        binaryWriter.Write(frameSize);
        binaryWriter.Write(bitsPerSample);
        binaryWriter.Write(0x61746164); // data
        binaryWriter.Write(dataChunkSize);

        foreach (var item in samples)
            binaryWriter.Write(item);

        return memoryStream.ToArray();
    }
}
