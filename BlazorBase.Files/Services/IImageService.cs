using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public interface IImageService
{
    Task CreateThumbnailAsync(byte[] inputImageBytes, int imageThumbnailSize, string destinationPath);
    Task<byte[]> ResizeImageAsync(byte[] inputImageBytes, int width, int height);
    Task<byte[]> ResizeImageToMaxSizeAsync(byte[] inputImageBytes, int maxSize);
    Task ResizeImageAsync(string path, int width, int height);
}
