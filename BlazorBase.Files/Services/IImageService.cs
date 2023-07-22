using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public interface IImageService
{
    Task CreateThumbnailAsync(byte[] inputImageBytes, int imageThumbnailSize, string destinationPath);
    byte[] ResizeImage(byte[] inputImageBytes, int width, int height);
    Task ResizeImageAsync(string path, int width, int height);
}
