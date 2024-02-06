using ImageMagick;
using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public class MagickImageService : IImageService
{
    public Task CreateThumbnailAsync(byte[] inputImageBytes, int imageThumbnailSize, string destinationPath)
    {
        using var image = new MagickImage(inputImageBytes);
        image.Resize(imageThumbnailSize, imageThumbnailSize); // Resize will keep the aspect ratio!
        return image.WriteAsync(destinationPath);
    }

    public Task<byte[]> ResizeImageAsync(byte[] inputImageBytes, int width, int height)
    {
        using var image = new MagickImage(inputImageBytes);
        image.Resize(width, height); // Resize will keep the aspect ratio!
        return Task.FromResult(image.ToByteArray());
    }

    public Task<byte[]> ResizeImageToMaxSizeAsync(byte[] inputImageBytes, int maxSize)
    {
        using var image = new MagickImage(inputImageBytes);
        if (image.Width <= maxSize && image.Height <= maxSize)
            return Task.FromResult(inputImageBytes);

        image.Resize(maxSize, maxSize); // Resize will keep the aspect ratio!
        return Task.FromResult(image.ToByteArray());
    }

    public Task ResizeImageAsync(string path, int width, int height)
    {
        using var image = new MagickImage(path);
        image.Resize(width, height); // Resize will keep the aspect ratio!
        return image.WriteAsync(path);
    }
}
