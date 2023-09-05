using ImageMagick;
using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public class MagickImageService : IImageService
{
    public Task CreateThumbnailAsync(byte[] inputImageBytes, int imageThumbnailSize, string destinationPath)
    {
        using var image = new MagickImage(inputImageBytes);
        image.Resize(imageThumbnailSize, imageThumbnailSize);
        return image.WriteAsync(destinationPath);
    }

    public byte[] ResizeImage(byte[] inputImageBytes, int width, int height)
    {
        using var image = new MagickImage(inputImageBytes);
        image.Resize(width, height);
        return image.ToByteArray();
    }

    public Task ResizeImageAsync(string path, int width, int height)
    {
        using var image = new MagickImage(path);
        image.Resize(width, height);
        return image.WriteAsync(path);
    }
}
