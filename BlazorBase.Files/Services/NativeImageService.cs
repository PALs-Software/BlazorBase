using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

// System.Drawing.Common currently not supporting webp images -> loading throws error !!!

public class NativeImageService : IImageService
{
    protected const string NotWindowsError = "This feature is only supported on Windows";

    public Task CreateThumbnailAsync(byte[] inputImageBytes, int imageThumbnailSize, string destinationPath)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException(NotWindowsError);

        var thumbnail = ResizeImage(inputImageBytes, imageThumbnailSize, imageThumbnailSize);
        return File.WriteAllBytesAsync(destinationPath, thumbnail);
    }

    public Task ResizeImageAsync(string path, int width, int height)
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

    public byte[] ResizeImage(byte[] inputImageBytes, int width, int height)
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

        return outputMemoryStream.ToArray();
    }

    public byte[] ResizeImageToMaxSize(byte[] inputImageBytes, int maxSize)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException(NotWindowsError);

        using var inputMemoryStream = new MemoryStream(inputImageBytes);
        var inputImage = Image.FromStream(inputMemoryStream);

        if (inputImage.Width <= maxSize && inputImage.Height <= maxSize)
            return inputImageBytes;

        var outputImage = ResizeImage(inputImage, maxSize, maxSize);
        using var outputMemoryStream = new MemoryStream();
        outputImage.Save(outputMemoryStream, inputImage.RawFormat);

        inputImage.Dispose();
        outputImage.Dispose();

        return outputMemoryStream.ToArray();
    }

    public Image ResizeImage(Image inputImage, int width, int height)
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
