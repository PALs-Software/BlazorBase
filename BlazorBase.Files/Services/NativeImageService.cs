using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

// System.Drawing.Common currently not supporting webp images -> loading throws error !!!
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen

public class NativeImageService : IImageService
{
    protected const string NotWindowsError = "This feature is only supported on Windows";

    public async Task CreateThumbnailAsync(byte[] inputImageBytes, uint imageThumbnailSize, string destinationPath)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException(NotWindowsError);

        var thumbnail = await ResizeImageAsync(inputImageBytes, imageThumbnailSize, imageThumbnailSize).ConfigureAwait(false);
        await File.WriteAllBytesAsync(destinationPath, thumbnail).ConfigureAwait(false);
    }

    public Task ResizeImageAsync(string path, uint width, uint height)
    {
        return Task.Run(() =>
        {
            if (!OperatingSystem.IsWindows())
                throw new NotSupportedException(NotWindowsError);

            var inputImage = Image.FromFile(path);
            var outputImage = ResizeImage(inputImage, width, height);
            outputImage.Save(path);

            inputImage.Dispose();
            outputImage.Dispose();
        });
    }

    public Task<byte[]> ResizeImageAsync(byte[] inputImageBytes, uint width, uint height)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException(NotWindowsError);

        using var inputMemoryStream = new MemoryStream(inputImageBytes);
        var inputImage = Image.FromStream(inputMemoryStream);
        var outputImage = ResizeImage(inputImage, width, height);
        using var outputMemoryStream = new MemoryStream();

        outputImage.Save(outputMemoryStream, inputImage.RawFormat);

        inputImage.Dispose();
        outputImage.Dispose();

        return Task.FromResult(outputMemoryStream.ToArray());
    }

    public Task<byte[]> ResizeImageToMaxSizeAsync(byte[] inputImageBytes, uint maxSize)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException(NotWindowsError);

        using var inputMemoryStream = new MemoryStream(inputImageBytes);
        var inputImage = Image.FromStream(inputMemoryStream);

        if (inputImage.Width <= maxSize && inputImage.Height <= maxSize)
            return Task.FromResult(inputImageBytes);

        var outputImage = ResizeImage(inputImage, maxSize, maxSize);
        using var outputMemoryStream = new MemoryStream();
        outputImage.Save(outputMemoryStream, inputImage.RawFormat);

        inputImage.Dispose();
        outputImage.Dispose();

        return Task.FromResult(outputMemoryStream.ToArray());
    }

    public Image ResizeImage(Image inputImage, uint width, uint height)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException(NotWindowsError);

        float widthRatio = width / (float)inputImage.Width;
        float heightRatio = height / (float)inputImage.Height;
        float finalRatio = heightRatio < widthRatio ? heightRatio : widthRatio;

        int destinationWidth = (int)(inputImage.Width * finalRatio);
        int destinationHeight = (int)(inputImage.Height * finalRatio);

        var resultImage = new Bitmap(destinationWidth, destinationHeight);

        var g = Graphics.FromImage(resultImage);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(inputImage, 0, 0, destinationWidth, destinationHeight);
        g.Dispose();

        return resultImage;
    }
}

#pragma warning restore CA1416 // Plattformkompatibilität überprüfen